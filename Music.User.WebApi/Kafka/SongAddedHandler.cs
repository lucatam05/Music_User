using System.Text.Json;
using Utility.Kafka.Abstractions.MessageHandlers;
using Music.User.Business.Abstractions;
using Music.Library.Shared.Events;

namespace MusicUser.Kafka;

public class SongAddedHandler(IBusiness business) : IMessageHandler<string, string>
{
    public async Task OnMessageReceivedAsync(string key, string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Messaggio ricevuto: {message}");
        var songAddedEvent = JsonSerializer.Deserialize<SongAddedEvent>(message);
        if (songAddedEvent is null)
        {
            Console.WriteLine("Deserializzazione fallita!");
            return;
        }
        Console.WriteLine($"UserId: {songAddedEvent.UserId}");
        await business.UpdateNumeroCanzoniAsync(songAddedEvent.UserId, cancellationToken);
    }
}