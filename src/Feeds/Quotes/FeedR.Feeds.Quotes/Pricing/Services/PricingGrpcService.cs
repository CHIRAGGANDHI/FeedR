using FeedR.Feeds.Quotes.Pricing.Models;
using FeedR.Feeds.Quotes.Pricing.Protos;
using Grpc.Core;
using System.Collections.Concurrent;

namespace FeedR.Feeds.Quotes.Pricing.Services;

public class PricingGrpcService : PricingFeed.PricingFeedBase
{
    private readonly BlockingCollection<CurrencyPair> _currencyPairs = new();

    private readonly IPricingGenerator _pricingGenerator;
    private readonly ILogger<PricingGrpcService> _logger;

    public PricingGrpcService(
        IPricingGenerator pricingGenerator,
        ILogger<PricingGrpcService> logger)
    {
        _pricingGenerator = pricingGenerator;
        _logger = logger;
    }

    public override Task<GetSymbolsResponse> GetSymbols(GetSymbolsRequest request, ServerCallContext context) 
        => Task.FromResult(new GetSymbolsResponse() 
                { 
                    Symbols =
                    { 
                        _pricingGenerator.GetSymbols() 
                    } 
                });

    public override async Task SubscribePricing(
        PricingRequest request, 
        IServerStreamWriter<PricingResponse> responseStream, 
        ServerCallContext context)
    {
        _logger.LogInformation("Started client streaming...");

        _pricingGenerator.PricingUpdated += OnPricingUpdated;

        while (context.CancellationToken.IsCancellationRequested == false)
        {
            if(_currencyPairs.TryTake(out var currencyPair) == false)
            {
                continue;
            }

            if(string.IsNullOrWhiteSpace(request.Symbol) == false && request.Symbol != currencyPair.Symbol)
            {
                continue;
            }

            await responseStream.WriteAsync(new PricingResponse
            {
                Symbol = currencyPair.Symbol,
                Value = (int) (100 * currencyPair.Value),
                Timestamp = currencyPair.Timestamp
            });
        }

        _pricingGenerator.PricingUpdated -= OnPricingUpdated;
        _logger.LogInformation("Stopped client streaming.");

        void OnPricingUpdated(object? sender, CurrencyPair currencyPair) => _currencyPairs.TryAdd(currencyPair);
    }
}
