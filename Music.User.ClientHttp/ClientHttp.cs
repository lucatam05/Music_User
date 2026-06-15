using Music.Catalogue.Shared;
using Music.Library.Shared;
using Music.Catalogue.Shared.Exceptions;
using Music.User.ClientHttp.Abstractions;

namespace Music.User.ClientHttp;

public class ClientHttp(Music.Library.ClientHttp.Abstractions.IClientHttp libraryClient, Music.Catalogue.ClientHttp.Abstractions.IClientHttp catalogueClient) : IClientHttp
{
    public async Task<List<SongDTO>?> GetCanzoniPopolariAsync(string token, CancellationToken cancellationToken)
    {
        var canzoni = await libraryClient.GetCanzoniUtenteAsync(token, cancellationToken);
        if (canzoni is null || !canzoni.Any())
            throw new ModelNotFoundException("Errore nella ricerca delle canzoni");

        var ultimoArtista = canzoni
            .OrderByDescending(c => c.DataAggiunta)
            .First()
            .Artista;
        if (ultimoArtista is null)
            throw new ModelNotFoundException("Ricerca dell'artista fallita");

        return await catalogueClient.SearchCanzoniPerArtistaAsync(ultimoArtista, cancellationToken);
    }

    public async Task<List<LibrarySongDTO>?> GetCanzoniUtenteAsync(string token, CancellationToken cancellationToken)
    {
        var canzoni = await libraryClient.GetCanzoniUtenteAsync(token, cancellationToken);
        if (canzoni is null || !canzoni.Any())
            throw new ModelNotFoundException("Errore nella ricerca delle canzoni");
        
        var ultime5Canzoni = canzoni
            .OrderByDescending(c => c.DataAggiunta)
            .Take(5)
            .ToList();

        if (ultime5Canzoni is null)
            throw new ModelNotFoundException("Errore nella ricerca delle canzoni");

        return ultime5Canzoni;
    }
}