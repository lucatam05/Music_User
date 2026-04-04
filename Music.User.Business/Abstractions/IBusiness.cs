using Music.User.Shared;

namespace Music.User.Business.Abstractions;

public interface IBusiness
{
    Task RegisterAsync(string nome, string cognome, DateTime dataNascita, string username,
        string email, string password, CancellationToken cancellationToken);

    Task<string?> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task UpdateNumeroCanzoniAsync(int id, CancellationToken cancellationToken);
}