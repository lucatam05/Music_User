using Music.Catalogue.Shared;
using Music.User.ClientHttp.Abstractions;

namespace Music.User.ClientHttp;

public class ClientHttp(HttpClient httpClient) : IClientHttp
{
    public async Task<List<SongDTO>?> GetCanzoniPopolariAsync(int id, CancellationToken cancellationToken)
    {
        //TODO
    }

    public async Task<List<SongDTO>?> GetCanzoniUtenteAsync(int id, CancellationToken cancellationToken)
    {
        //TODO
    }
}