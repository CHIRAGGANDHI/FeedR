using FeedR.Aggregator.Services.Models;

namespace FeedR.Aggregator.Services;

public interface IPricingHandler
{
    Task HandleAsync(CurrencyPair currencyPair);
}
