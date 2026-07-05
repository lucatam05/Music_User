namespace MusicUser.Kafka;
using Utility.Kafka.Abstractions.MessageHandlers;

public class MessageHandlerFactory : IMessageHandlerFactory<string, string>
{
    public IMessageHandler<string, string> Create(string topic, IServiceProvider serviceProvider)
    {
        return topic switch
        {
            "song-added-to-library" => serviceProvider.GetRequiredService<SongAddedHandler>(),
            
            "song-removed-from-library" => serviceProvider.GetRequiredService<SongRemovedHandler>(),
            
            _ => throw new ArgumentException($"Topic non gestito: {topic}")
            
        };
    }
}