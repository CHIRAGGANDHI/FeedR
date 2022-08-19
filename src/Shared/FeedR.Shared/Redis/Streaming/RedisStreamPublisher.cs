using FeedR.Shared.Serialization;
using FeedR.Shared.Streaming;
using StackExchange.Redis;

namespace FeedR.Shared.Redis.Streaming;

public class RedisStreamPublisher : IStreamPublisher
{
    private readonly ISubscriber _subscriber;
    private readonly ISerializer _serializer;

    public RedisStreamPublisher(
        IConnectionMultiplexer connectionMulptiplexer,
        ISerializer serializer)
    {
        _subscriber = connectionMulptiplexer.GetSubscriber();
        _serializer = serializer;
    }

    public Task PublishAsync<T>(string topic, T data) where T : class
    {
        var payload = _serializer.Serialize(data);
        return _subscriber.PublishAsync(topic, payload);
    }
}
