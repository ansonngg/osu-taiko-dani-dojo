using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Infrastructure.Service;
using OsuTaikoDaniDojo.Presentation.Middleware;

namespace OsuTaikoDaniDojo.Presentation.Utility;

public static class DependencyInjection
{
    public static void AddOptions(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<OsuOptions>(builder.Configuration.GetSection("Osu"));
        builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
    }

    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IOsuAuthService, OsuAuthService>();
        builder.Services.AddHttpClient<IRedisSessionService, RedisSessionService>();
    }

    public static void UseMiddleware(this WebApplication app)
    {
        app.UseMiddleware<SessionMiddleware>(
            app.Services.GetRequiredService<IOsuAuthService>(),
            app.Services.GetRequiredService<IRedisSessionService>());
    }
}
