using System.Collections.Concurrent;
using WeatherApi.ApiClients;

namespace WeatherApi.Services
{
    public class CachedWeatherService(IWeatherService apiClient) : IWeatherService
    {
        private record CacheEntry(OpenWeatherApiResponse Response, DateTime CreatedAtUtc);
        private static readonly ConcurrentDictionary<string, CacheEntry> Cache = new();
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

        public async Task<OpenWeatherApiResponse?> GetWeatherAsync(string queryKey, string requestUriQueryString)
        {

            if (Cache.TryGetValue(queryKey, out var entry) && IsFresh(entry))
            {
                return entry.Response;
            }

            var semaphore = Locks.GetOrAdd(queryKey, _ => new SemaphoreSlim(1, 1));

            var acquired = await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
            if (!acquired)
            {
                throw new TimeoutException($"Could not acquire lock for weather query: {queryKey}. Try again later.");
            }

            try
            {
                // check cache again after acquiring the lock to avoid race conditions
                // from simultaneous requests when the cache expired
                // one request may have already updated the cache while waiting for the lock

                if (Cache.TryGetValue(queryKey, out entry) && IsFresh(entry))
                {
                    return entry.Response;
                }

                var apiResponse = await apiClient.GetWeatherAsync(queryKey, requestUriQueryString);

                if (apiResponse != null)
                {
                    Cache.AddOrUpdate(
                        queryKey,
                        _ => new CacheEntry(apiResponse, DateTime.UtcNow),
                        (_, _) => new CacheEntry(apiResponse, DateTime.UtcNow));
                }

                return apiResponse;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static bool IsFresh(CacheEntry entry)
        {
            return DateTime.UtcNow - entry.CreatedAtUtc < CacheDuration;
        }
    }
}
