using Microsoft.Extensions.Options;
using System.Text.Json;
using WeatherApi.Services;
using WeatherApi.Settings;

namespace WeatherApi.ApiClients
{
    public class OpenWeatherApiClient : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenWeatherApiClient> _logger;
        private readonly string _apiKey;

        public OpenWeatherApiClient(HttpClient httpClient, IOptions<OpenWeatherApiClientSettings> options, ILogger<OpenWeatherApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var settings = options.Value;

            if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            {
                throw new InvalidOperationException("OpenWeather Base URL is missing in configuration or user secrets.");
            }

            if (string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                throw new InvalidOperationException("OpenWeather API Key is missing in configuration or user secrets.");
            }

            _apiKey = settings.ApiKey;
            _httpClient.BaseAddress = new Uri(settings.BaseUrl);
        }

        public async Task<OpenWeatherApiResponse?> GetWeatherAsync(string queryKey, string requestUriQueryString)
        {
            var fullUrl = $"weather?{requestUriQueryString}&appid={_apiKey}";

            _logger.LogInformation("Fetching fresh data from OpenWeather API for query: {QueryKey}", queryKey);

            var response = await _httpClient.GetAsync(fullUrl);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OpenWeatherApiResponse>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            _logger.LogWarning("OpenWeather API failed with status code {StatusCode} for query {QueryKey}", response.StatusCode, queryKey);
            return null;

        }
    }
}
