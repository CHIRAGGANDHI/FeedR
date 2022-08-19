namespace FeedR.Feeds.Quotes.Pricing.Models;

public sealed record CurrencyPair(string Symbol, decimal Value, long Timestamp);
