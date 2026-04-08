using Music.Catalogue.Shared;

namespace Music.User.ClientHttp.Abstractions;

public interface IClientHttp
{
    //ritornare canzoni popolari, magari dell'ultimo artista aggiunto
    Task<List<SongDTO>?> GetCanzoniPopolariAsync(int id, CancellationToken cancellationToken);
    
    //ritornare le ultime 5 canzoni aggiunte alla libreria
    Task<List<SongDTO>?> GetCanzoniUtenteAsync(int id, CancellationToken cancellationToken);
}