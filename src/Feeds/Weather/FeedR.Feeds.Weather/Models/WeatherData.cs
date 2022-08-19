namespace FeedR.Feeds.Weather.Models;

public record WeatherData(string Location, double Temperature, double Humidity, double Wind, string Condition);
