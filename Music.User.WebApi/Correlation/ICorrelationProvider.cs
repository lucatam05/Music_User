namespace MusicUser.Correlation;

/// <summary>
/// Espone il CorrelationId della richiesta corrente. Fonte di verità unica,
/// usata sia dal middleware/log context sia dal DelegatingHandler HTTP in uscita.
/// </summary>
public interface ICorrelationIdProvider
{
    string? CorrelationId { get; }
}