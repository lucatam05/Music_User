using System.Text.Json;
using Utility.Kafka.Abstractions.MessageHandlers;
using Music.User.Business.Abstractions;
using Music.Library.Shared.Events;

namespace MusicUser.Kafka;

public class SongRemovedHandler(IBusiness business) : IMessageHandler<string, string>
{
    public async Task OnMessageReceivedAsync(string key, string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Messaggio ricevuto: {message}");
        var songRemovedEvent = JsonSerializer.Deserialize<SongRemovedEvent>(message);
        if (songRemovedEvent is null)
        {
            Console.WriteLine("Deserializzazione fallita!");
            return;
        }
        Console.WriteLine($"UserId: {songRemovedEvent.UserId}");
        await business.DecrementNumeroCanzoniAsync(songRemovedEvent.UserId, cancellationToken);
    }
}