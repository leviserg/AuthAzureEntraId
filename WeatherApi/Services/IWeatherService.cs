using WeatherApi.ApiClients;

namespace WeatherApi.Services
{
    public interface IWeatherService
    {
        public Task<OpenWeatherApiResponse?> GetWeatherAsync(string queryKey, string requestUriQueryString);
    }
}
