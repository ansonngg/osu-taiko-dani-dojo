using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Infrastructure.Service;
using OsuTaikoDaniDojo.Web.Middleware;
using SessionOptions = OsuTaikoDaniDojo.Application.Options.SessionOptions;

namespace OsuTaikoDaniDojo.Web.Utility;

public static class DependencyInjection
{
    public static void AddOptions(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<SessionOptions>(builder.Configuration.GetSection("Session"));
        builder.Services.Configure<OsuOptions>(builder.Configuration.GetSection("Osu"));
        builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpClient<RedisSessionService>();
        services.AddHttpClient<IOsuAuthService, OsuAuthService>();
        services.AddSingleton<ISessionService, HybridSessionService>();
    }

    public static void UseMiddleware(this WebApplication app)
    {
        app.UseMiddleware<SessionMiddleware>();
    }
}
