using System.Text.Json.Serialization;

namespace WeatherApi.ApiClients
{
    public record OpenWeatherApiResponse
    (
        [property: JsonPropertyName("coord")] CoordinateDto Coordinate,
        [property: JsonPropertyName("weather")] List<WeatherDetailDto> Weather,
        [property: JsonPropertyName("base")] string Base,
        [property: JsonPropertyName("main")] MainDto MainWeatherData,
        [property: JsonPropertyName("visibility")] int Visibility,
        [property: JsonPropertyName("wind")] WindDto Wind,
        [property: JsonPropertyName("clouds")] CloudsDto Clouds,
        [property: JsonPropertyName("dt")] long Dt,
        [property: JsonPropertyName("sys")] SysDto SystemData,
        [property: JsonPropertyName("timezone")] int Timezone,
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("cod")] int Code
    );

    public record CoordinateDto(
        [property: JsonPropertyName("lon")] double Longitude,
        [property: JsonPropertyName("lat")] double Latitude
    );
    public record WeatherDetailDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("main")] string Main,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("icon")] string Icon
    );
    public record MainDto(
        [property: JsonPropertyName("temp")] double TemperatureKelvin,
        [property: JsonPropertyName("feels_like")] double TemperatureKelvinFeelsLike,
        [property: JsonPropertyName("temp_min")] double TemperatureKelvinMin,
        [property: JsonPropertyName("temp_max")] double TemperatureKelvinMax,
        [property: JsonPropertyName("pressure")] int PressureMilliBar,
        [property: JsonPropertyName("humidity")] int HumidityPercentage
    );
    public record WindDto(
        [property: JsonPropertyName("speed")] double Speed,
        [property: JsonPropertyName("deg")] int Deg,
        [property: JsonPropertyName("gust")] double? Gust
    );
    public record CloudsDto([property: JsonPropertyName("all")] int All);
    public record SysDto(
        [property: JsonPropertyName("country")] string Country,
        [property: JsonPropertyName("sunrise")] long Sunrise,
        [property: JsonPropertyName("sunset")] long Sunset
    );
}
