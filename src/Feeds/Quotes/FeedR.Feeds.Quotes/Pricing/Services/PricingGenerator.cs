using FeedR.Feeds.Quotes.Pricing.Models;

namespace FeedR.Feeds.Quotes.Pricing.Services;

public class PricingGenerator : IPricingGenerator
{
    private readonly Dictionary<string, decimal> _currencyPairs = new()
    {
        ["EURUSD"] = 1.13M,
        ["EURGBP"] = 0.85M,
        ["EURCHF"] = 1.04M,
        ["EURPLN"] = 4.62M,
    };

    private readonly ILogger<PricingGenerator> _logger;
    private readonly Random _random = new Random();

    private bool _isRunning;

    public event EventHandler<CurrencyPair>? PricingUpdated;

    public PricingGenerator(ILogger<PricingGenerator> logger)
    {
        _logger = logger;
    }    

    public IEnumerable<string> GetSymbols() => _currencyPairs.Keys;

    public async IAsyncEnumerable<CurrencyPair> StartAsync()
    {
        _isRunning = true;

        while (_isRunning)
        {
            foreach(var (symbol, pricing) in _currencyPairs)
            {
                if (_isRunning == false)
                {
                    yield break;
                }

                var newPricing = pricing + NextTick();
                _currencyPairs[symbol] = newPricing;

                var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                _logger.LogInformation($"Updated pricing for: {symbol}, {pricing:F} -> {newPricing:F}");
                var currencyPair = new CurrencyPair(symbol, newPricing, timeStamp);

                PricingUpdated?.Invoke(this, currencyPair);

                yield return currencyPair;

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }        
    }

    public Task StopAsync()
    {
        _isRunning = false;
        return Task.CompletedTask;
    }

    private decimal NextTick()
    {
        var sign = _random.Next(0, 2) == 0 ? -1 : 1;
        var tick = _random.NextDouble() / 20;
        return (decimal)(sign * tick);
    }
}
