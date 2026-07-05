using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Music.User.Repository.Abstractions;
using Music.User.Repository.Model;
using Music.User.Shared.Exceptions;

namespace Music.User.Repository;

public class Repository(UserDbContext userDbContext) : IRepository
{
    public async Task<Users?> GetUserPerUsernameAsync(string username, CancellationToken cancellationToken)
    {
         return await userDbContext.UsersEnumerable.
             FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<Users?> GetUserPerEmailAsync(string mail, CancellationToken cancellationToken)
    {
        return await userDbContext.UsersEnumerable.
            FirstOrDefaultAsync(u => u.Email == mail, cancellationToken);
    }

    public async Task<Users> InsertUserAsync(string nome, string cognome, DateTime dataNascita, string username,
        string email, string password, CancellationToken cancellationToken)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);        
        byte[] hash = pbkdf2.GetBytes(20);
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);
        string savedPasswordHash = Convert.ToBase64String(hashBytes);
        
        Users user = new Users
        {
            Nome = nome,
            Cognome = cognome,
            DataNascita = dataNascita,
            Email = email,
            Username = username,
            Password = savedPasswordHash
        };
        
        userDbContext.Add(user);
        await userDbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task IncrementNumeroCanzoniAsync(int id, CancellationToken cancellationToken)
    {
        Users? user = await GetUserPerId(id, cancellationToken);
        if (user is null)
            throw new ModelNotFoundException("Utente non trovato");
        user.NumeroCanzoni += 1;
        await userDbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DecrementNumeroCanzoniAsync(int id, CancellationToken cancellationToken)
    {
        Users? user = await GetUserPerId(id, cancellationToken);
        if (user is null)
            throw new ModelNotFoundException("Utente non trovato");
        user.NumeroCanzoni -= 1;
        await userDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Users?> GetUserPerId(int id, CancellationToken cancellationToken)
    {
        return await userDbContext.UsersEnumerable.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}