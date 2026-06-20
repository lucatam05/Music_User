using Music.User.Repository.Model;

namespace Music.User.Repository.Abstractions;

public interface IRepository
{
    Task<Users?> GetUserPerUsernameAsync(string username, CancellationToken cancellationToken);
    Task<Users?> GetUserPerEmailAsync(string mail, CancellationToken cancellationToken);
    Task<Users> InsertUserAsync(string nome, string cognome, DateTime dataNascita, string email, string username,
        string password, CancellationToken cancellationToken);
    Task UpdateNumeroCanzoniAsync(int id, CancellationToken cancellationToken);
}