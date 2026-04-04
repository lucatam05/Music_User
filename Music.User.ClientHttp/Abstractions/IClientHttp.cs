namespace Music.User.ClientHttp.Abstractions;

public class IClientHttp
{
    Task<List<>?> GetCanzoniUtenteAsync(int userId, CancellationToken cancellationToken);
}