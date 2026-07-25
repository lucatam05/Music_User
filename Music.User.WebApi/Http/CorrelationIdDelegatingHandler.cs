using MusicUser.Middlewares;

namespace MusicUser.Http;

/// <summary>
/// Copia l'header di correlazione dalla richiesta in ingresso corrente (se presente)
/// su ogni richiesta HTTP in uscita verso altri servizi interni (Library, Catalogue),
/// così i log restano correlabili tra i vari servizi coinvolti nella stessa catena di chiamate.
/// </summary>
public class CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.HeaderName] as string;

        if (!string.IsNullOrWhiteSpace(correlationId) && !request.Headers.Contains(CorrelationIdMiddleware.HeaderName))
        {
            request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
