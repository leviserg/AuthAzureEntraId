using WeatherApi.ApiClients;
using WeatherApi.Services;

namespace WeatherApiTests
{
    public class CachedWeatherServiceTests
    {
        [Fact]
        public async Task GetWeatherAsync_FirstCall_CallsInnerServiceAndReturnsResponse()
        {
            // Arrange
            var fake = new DelegateWeatherService((_, _) => Task.FromResult<OpenWeatherApiResponse?>(CreateResponse("first")));
            var sut = new CachedWeatherService(fake);

            var key = $"key-{Guid.NewGuid()}";

            // Act
            var result = await sut.GetWeatherAsync(key, "q=kyiv");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("first", result.Name);
            Assert.Equal(1, fake.CallCount);
        }

        [Fact]
        public async Task GetWeatherAsync_SecondCallWithSameKey_ReturnsCachedResponse_AndDoesNotCallInnerServiceAgain()
        {
            // Arrange
            var fake = new DelegateWeatherService((_, _) =>
            {
                var name = $"call-{Interlocked.Increment(ref _fakeCounter)}";
                return Task.FromResult<OpenWeatherApiResponse?>(CreateResponse(name));
            });

            var sut = new CachedWeatherService(fake);
            var key = $"key-{Guid.NewGuid()}";

            // Act
            var first = await sut.GetWeatherAsync(key, "q=kyiv");
            var second = await sut.GetWeatherAsync(key, "q=kyiv");

            // Assert
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Same(first, second);
            Assert.Equal(1, fake.CallCount);
        }

        [Fact]
        public async Task GetWeatherAsync_NullResponse_IsNotCached()
        {
            // Arrange
            var fake = new DelegateWeatherService((_, _) => Task.FromResult<OpenWeatherApiResponse?>(null));
            var sut = new CachedWeatherService(fake);
            var key = $"key-{Guid.NewGuid()}";

            // Act
            var first = await sut.GetWeatherAsync(key, "q=kyiv");
            var second = await sut.GetWeatherAsync(key, "q=kyiv");

            // Assert
            Assert.Null(first);
            Assert.Null(second);
            Assert.Equal(2, fake.CallCount);
        }

        [Fact]
        public async Task GetWeatherAsync_ConcurrentCallsForSameKey_CallInnerServiceOnlyOnce()
        {
            // Arrange
            var fake = new DelegateWeatherService(async (_, _) =>
            {
                await Task.Delay(150);
                return CreateResponse("concurrent");
            });

            var sut = new CachedWeatherService(fake);
            var key = $"key-{Guid.NewGuid()}";

            // Act
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => sut.GetWeatherAsync(key, "q=kyiv"))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.All(results, r => Assert.NotNull(r));
            Assert.All(results, r => Assert.Equal("concurrent", r!.Name));
            Assert.Equal(1, fake.CallCount);
        }

        private static int _fakeCounter;

        private static OpenWeatherApiResponse CreateResponse(string name) =>
            new(
                Coordinate: new CoordinateDto(30.52, 50.45),
                Weather: [new WeatherDetailDto(800, "Clear", "clear sky", "01d")],
                Base: "stations",
                MainWeatherData: new MainDto(293.15, 292.15, 291.15, 294.15, 1012, 55),
                Visibility: 10000,
                Wind: new WindDto(2.5, 150, null),
                Clouds: new CloudsDto(0),
                Dt: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                SystemData: new SysDto("UA", 1_700_000_000, 1_700_040_000),
                Timezone: 10800,
                Id: 703448,
                Name: name,
                Code: 200
            );

        private sealed class DelegateWeatherService(Func<string, string, Task<OpenWeatherApiResponse?>> impl) : IWeatherService
        {
            private readonly Func<string, string, Task<OpenWeatherApiResponse?>> _impl = impl;
            private int _callCount;

            public int CallCount => _callCount;

            public async Task<OpenWeatherApiResponse?> GetWeatherAsync(string queryKey, string requestUriQueryString)
            {
                Interlocked.Increment(ref _callCount);
                return await _impl(queryKey, requestUriQueryString);
            }
        }

    }
}
