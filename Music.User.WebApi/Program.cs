using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Music.User.Business;
using Music.User.Business.Abstractions;
using Music.User.Repository;
using Music.User.Repository.Abstractions;
using MusicUser.Kafka;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

// ClientHttp verso LibraryService
builder.Services.AddHttpClient<Music.Library.ClientHttp.Abstractions.IClientHttp, Music.Library.ClientHttp.ClientHttp>("LibraryClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Library"]!);
});

//ClientHttp verso CatalogueService
builder.Services.AddHttpClient<Music.Catalogue.ClientHttp.Abstractions.IClientHttp, Music.Catalogue.ClientHttp.ClientHttp>("CatalogueClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Catalogue"]!);
});

// ClientHttp del UserService (orchestratore)
builder.Services.AddScoped<Music.User.ClientHttp.Abstractions.IClientHttp, Music.User.ClientHttp.ClientHttp>();

builder.Services.AddScoped<SongAddedHandler>();
builder.Services.AddScoped<SongRemovedHandler>();

builder.Services.AddKafkaConsumerService<UserKafkaTopics, MessageHandlerFactory>(builder.Configuration);

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

Console.WriteLine($"Kafka BootstrapServers: {builder.Configuration["Kafka:ConsumerClient:BootstrapServers"]}");
Console.WriteLine($"Kafka GroupId: {builder.Configuration["Kafka:ConsumerClient:GroupId"]}");
var app = builder.Build();

// Migrations
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
db.Database.Migrate();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();