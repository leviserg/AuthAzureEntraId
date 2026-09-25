using WeatherApi.Services;
using Microsoft.Identity.Web.Resource;

namespace WeatherApi
{
    public static class GetWeatherEndpoint
    {
        public static void BuildWeatherEndpoint(this IEndpointRouteBuilder app) {
            app.MapGet("/api/weather",
                [RequiredScopeOrAppPermission(
                    RequiredScopesConfigurationKey = "AzureAd:Scopes",
                    RequiredAppPermissionsConfigurationKey = "AzureAd:AppRoles")]
                async (string? city, double? lat, double? lon,
                IWeatherService weatherService
                //HttpContext context, 
                //ILogger<Program> logger
            ) =>
            {

                /*
                var user = context.User;

                var userName = user.GetDisplayName() ?? user.FindFirst("name")?.Value ?? "Unknown User"; // email

                var userFullName = user.Claims.Where(c => c.Type == "name").Select(c => c.Value).FirstOrDefault() ?? "Unknown Full Name";
                var userId = user.GetObjectId() ?? "Unknown UserID";
                var userIPAddress = user.Claims.Where(c => c.Type == "ipaddr").Select(c => c.Value).FirstOrDefault() ?? "Unknown Full Name"; ;

                logger.LogInformation("Weather request received from User: {UserName} ({UserFullName}) with Object ID: {UserId} and IP Address: {UserIPAddress}",
                    userName, userFullName, userId, userIPAddress);
                */

                string queryKey;
                string queryString;

                if (!string.IsNullOrWhiteSpace(city))
                {
                    queryKey = $"city-{city.Trim().ToLowerInvariant()}";
                    queryString = $"q={Uri.EscapeDataString(city)}";
                }
                else if (lat.HasValue && lon.HasValue)
                {
                    queryKey = $"coord-{lat.Value:F4},{lon.Value:F4}";
                    queryString = $"lat={lat.Value}&lon={lon.Value}";
                }
                else
                {
                    return Results.BadRequest(new
                    { error = "You must provide either a 'city' parameter or both 'lat' and 'lon' coordinates." });
                }

                var weatherData = await weatherService.GetWeatherAsync(queryKey, queryString);

                return weatherData == null
                    ? Results.NotFound(new { error = "Weather data could not be retrieved." })
                    : Results.Ok(weatherData);
            })
            .RequireAuthorization();
        }
    }
}
