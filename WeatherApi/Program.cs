using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using WeatherApi;
using WeatherApi.ApiClients;
using WeatherApi.Services;
using WeatherApi.Settings;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.Configure<OpenWeatherApiClientSettings>(
    builder.Configuration.GetSection(OpenWeatherApiClientSettings.SectionName));

builder.Services.AddHttpClient<OpenWeatherApiClient>();

builder.Services.AddScoped<IWeatherService>(provider =>
{
    var apiClient = provider.GetRequiredService<OpenWeatherApiClient>();
    return new CachedWeatherService(apiClient);
});

builder.Services.AddAuthorization();


var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages(async context =>
{
    context.HttpContext.Response.ContentType = "application/json";
    switch (context.HttpContext.Response.StatusCode)
    {
        case StatusCodes.Status401Unauthorized:
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                Error = "Unauthorized",
                Message = "A valid Microsoft Entra bearer token was missing or malformed."
            });
            break;
        case StatusCodes.Status403Forbidden:
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                Error = "Forbidden",
                Message = "The provided token does not contain the required scope ('Weather.Read') or app role ('Weather.Read.All')."
            });
            break;
    }
});


// ..api/weather?city=Paris; ..api/weather?city=London

app.BuildWeatherEndpoint();

app.Run();