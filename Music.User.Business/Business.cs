using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Music.Catalogue.Shared;
using Music.Library.Shared;
using Music.User.Business.Abstractions;
using Music.User.ClientHttp.Abstractions;
using Music.User.Repository.Abstractions;
using Music.User.Repository.Model;
using Music.User.Shared.Exceptions;

namespace Music.User.Business;

public class Business(IRepository repository, IConfiguration configuration, IClientHttp clientHttp, Music.Library.ClientHttp.Abstractions.IClientHttp libraryClient) : IBusiness
{
    public async Task RegisterAsync(string nome, string cognome, DateTime dataNascita, string username, string email, string password,
        CancellationToken cancellationToken)
    {
        Users? user = await repository.GetUserPerEmailAsync(email, cancellationToken);
        if (user is not null)
            throw new DoubleRegisterException("Utente già registrato");

        var newUser = await repository.InsertUserAsync(nome, cognome, dataNascita, username, email, password, cancellationToken);
        await libraryClient.CreateLibraryAsync(newUser.Id, cancellationToken);
    }

    public async Task<string?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        Users? user = await repository.GetUserPerEmailAsync(email, cancellationToken);
        if (user is null)
            throw new ModelNotFoundException("Utente non registrato!");
        
        byte[] hashPassword = Convert.FromBase64String(user.Password);
        byte[] salt = new byte[16];
        Array.Copy(hashPassword, 0, salt, 0, 16);
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);        
        byte[] hash = pbkdf2.GetBytes(20);
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);
        string savedPasswordHash = Convert.ToBase64String(hashBytes);

        if (user.Password != savedPasswordHash)
            throw new ModelNotFoundException("Password non corretta!");
        
        return GenerateJwtToken(user);
    }
    
    public async Task<List<LibrarySongDTO>?> GetCanzoniUtenteAsync(string token, CancellationToken cancellationToken)
    {
        return await clientHttp.GetCanzoniUtenteAsync(token, cancellationToken);
    }

    public async Task<List<SongDTO>?> GetCanzoniPopolariAsync(string token, CancellationToken cancellationToken)
    {
        return await clientHttp.GetCanzoniPopolariAsync(token, cancellationToken);
    }

    public async Task UpdateNumeroCanzoniAsync(int id, CancellationToken cancellationToken)
    {
        await repository.UpdateNumeroCanzoniAsync(id, cancellationToken);
    }
    
    private string GenerateJwtToken(Users user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
    
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username)
        };
    
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );
    
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}