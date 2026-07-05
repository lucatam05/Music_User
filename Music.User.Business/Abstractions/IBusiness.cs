using Music.Catalogue.Shared;
using Music.Library.Shared;

namespace Music.User.Business.Abstractions;

public interface IBusiness
{
    Task RegisterAsync(string nome, string cognome, DateTime dataNascita, string username,
        string email, string password, CancellationToken cancellationToken);

    Task<string?> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<List<LibrarySongDTO>?> GetCanzoniUtenteAsync(string token, CancellationToken cancellationToken);
    Task<List<SongDTO>?> GetCanzoniPopolariAsync(string token, CancellationToken cancellationToken);
    Task IncrementNumeroCanzoniAsync(int id, CancellationToken cancellationToken);
    Task DecrementNumeroCanzoniAsync(int id, CancellationToken cancellationToken);
}