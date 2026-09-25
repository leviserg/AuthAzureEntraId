namespace WeatherApi.Settings
{
    public class OpenWeatherApiClientSettings
    {
        public const string SectionName = "OpenWeather";

        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
