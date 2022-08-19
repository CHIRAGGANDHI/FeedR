using FeedR.Feeds.Quotes.Pricing.Requests;
using System.Threading.Channels;

namespace FeedR.Feeds.Quotes.Pricing.Services;

public sealed class PricingRequestsChannel
{
    public readonly Channel<IPricingRequest> Requests = Channel.CreateUnbounded<IPricingRequest>();
}
