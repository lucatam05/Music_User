using System.Text.Json;
using Serilog.Context;
using Utility.Kafka.Abstractions.MessageHandlers;
using Music.User.Business.Abstractions;
using Music.Library.Shared.Events;

namespace MusicUser.Kafka;

public class SongRemovedHandler(IBusiness business) : IMessageHandler<string, string>
{
    public async Task OnMessageReceivedAsync(string key, string message, CancellationToken cancellationToken = default)
    {
        var songRemovedEvent = JsonSerializer.Deserialize<SongRemovedEvent>(message);
        if (songRemovedEvent is null)
        {
            return;
        }

        // Riaggancia il CorrelationId originato dalla richiesta HTTP lato LibraryService,
        // così i log di questo consumer risultano correlabili a quelli del producer
        using (LogContext.PushProperty("CorrelationId", songRemovedEvent.CorrelationId))
        {
            await business.DecrementNumeroCanzoniAsync(songRemovedEvent.UserId, cancellationToken);
        }
    }
}