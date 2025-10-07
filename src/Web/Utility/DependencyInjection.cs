using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Infrastructure.Repository;
using OsuTaikoDaniDojo.Infrastructure.Service;
using OsuTaikoDaniDojo.Web.Middleware;
using Supabase;
using SessionOptions = OsuTaikoDaniDojo.Application.Options.SessionOptions;

namespace OsuTaikoDaniDojo.Web.Utility;

public static class DependencyInjection
{
    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(
            _ =>
            {
                var databaseOptions = builder.Configuration.GetSection("Database").Get<DatabaseOptions>();

                if (databaseOptions == null)
                {
                    throw new NullReferenceException("Database options is null.");
                }

                var client = new Client(
                    databaseOptions.Url,
                    databaseOptions.Key,
                    new SupabaseOptions { AutoConnectRealtime = false });

                client.InitializeAsync().Wait();
                return client;
            });
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddHttpClient<RedisSessionService>();
        services.AddHttpClient<IOsuAuthService, OsuAuthService>();
        services.AddHttpClient<IOsuMultiplayerRoomService, OsuMultiplayerRoomService>();
        services.AddSingleton<ISessionService, HybridSessionService>();
    }

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IExamRepository, ExamRepository>();
        services.AddSingleton<IExamSessionRepository, ExamSessionRepository>();
    }

    public static void AddUtilities(this IServiceCollection services)
    {
        services.AddOptions<SessionOptions>().BindConfiguration("Session");
        services.AddOptions<OsuOptions>().BindConfiguration("Osu");
        services.AddOptions<RedisOptions>().BindConfiguration("Redis");
        services.AddMemoryCache();
    }

    public static void UseMiddleware(this WebApplication app)
    {
        app.UseMiddleware<SessionMiddleware>();
    }
}
