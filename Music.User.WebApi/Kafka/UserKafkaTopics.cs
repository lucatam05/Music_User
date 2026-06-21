namespace MusicUser.Kafka;

public class UserKafkaTopics : AbstractKafkaTopics
{
    public string SongAdded { get; set; } = "song-added-to-library";
    
    public override IEnumerable<string> GetTopics()
    {
        yield return SongAdded;
    }
}