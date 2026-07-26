using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Music.User.Business;
using Music.User.Business.Abstractions;
using Music.User.Repository;
using Music.User.Repository.Abstractions;
using MusicUser;
using MusicUser.Correlation;
using MusicUser.HealthChecks;
using MusicUser.Kafka;
using MusicUser.Middlewares;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("ServiceName", "UserService")
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Avvio di UserService...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // DbContext
    builder.Services.AddDbContext<UserDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // DI
    builder.Services.AddScoped<IRepository, Repository>();
    builder.Services.AddScoped<IBusiness, Business>();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();

    builder.Services.AddResilientHttpClients(builder.Configuration);

    // ClientHttp del UserService (orchestratore)
    builder.Services.AddScoped<Music.User.ClientHttp.Abstractions.IClientHttp, Music.User.ClientHttp.ClientHttp>();

    builder.Services.AddTransient<SongAddedHandler>();
    builder.Services.AddTransient<SongRemovedHandler>();

    builder.Services.AddKafkaConsumerService<UserKafkaTopics, MessageHandlerFactory>(builder.Configuration);

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<UserDbContext>("database")
        .AddCheck<KafkaHealthCheck>("kafka");

    // JWT Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),

                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],

                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Music User API",
            Version = "v1",
            Description = "API per la gestione dell'utente"
        });

        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Inserisci il token nel formato: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Migrations
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        db.Database.Migrate();
    }

    // Deve precedere UseSerilogRequestLogging per far sì che anche la riga di log
    // riassuntiva della richiesta sia arricchita con il CorrelationId
    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync
        })
        .AllowAnonymous();

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "UserService terminato in modo inatteso durante l'avvio");
}
finally
{
    Log.CloseAndFlush();
}