using FeedR.Shared.Serialization;
using FeedR.Shared.Streaming;
using StackExchange.Redis;

namespace FeedR.Shared.Redis.Streaming;

public class RedisStreamSubscriber : IStreamSubscriber
{
    private readonly ISubscriber _subscriber;
    private readonly ISerializer _serializer;

    public RedisStreamSubscriber(
        IConnectionMultiplexer connectionMulptiplexer,
        ISerializer serializer)
    {
        _subscriber = connectionMulptiplexer.GetSubscriber();
        _serializer = serializer;
    }

    public Task SubscribeAsync<T>(string topic, Action<T> handler) where T : class 
        => _subscriber.SubscribeAsync(topic, (_, data) =>
            {
                var payload = _serializer.Deserialize<T>(data);

                if (payload is null)
                {
                    return;
                }

                handler(payload);
            });
}
