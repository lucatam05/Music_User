using Music.Catalogue.Shared;

namespace Music.User.ClientHttp.Abstractions;

public interface IClientHttp
{
    Task<List<SongDTO>?> GetCanzoniUtenteAsync(int userId, CancellationToken cancellationToken);
}