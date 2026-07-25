using MusicUser.Middlewares;

namespace MusicUser.Correlation;

public class CorrelationIdProvider(IHttpContextAccessor httpContextAccessor) : ICorrelationIdProvider
{
    public string? CorrelationId => httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.HeaderName] as string;
}