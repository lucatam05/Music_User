using Music.Catalogue.Shared;
using Music.Library.Shared;

namespace Music.User.ClientHttp.Abstractions;

public interface IClientHttp
{
    //ritornare canzoni popolari, magari dell'ultimo artista aggiunto
    Task<List<SongDTO>?> GetCanzoniPopolariAsync(string artista, CancellationToken cancellationToken);
    
    //ritornare le ultime 5 canzoni aggiunte alla libreria
    Task<List<LibrarySongDTO>?> GetCanzoniUtenteAsync(string token, CancellationToken cancellationToken);
}