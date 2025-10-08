using Microsoft.AspNetCore.Authentication;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Infrastructure.Repository;
using OsuTaikoDaniDojo.Infrastructure.Service;
using OsuTaikoDaniDojo.Web.Handler;
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
        services.AddHttpClient<IOsuAuthService, OsuAuthService>(
            client => { client.BaseAddress = new Uri("https://osu.ppy.sh"); });

        services.AddHttpClient<IOsuMultiplayerRoomService, OsuMultiplayerRoomService>(
                client => { client.BaseAddress = new Uri("https://osu.ppy.sh/api/v2/rooms/"); })
            .AddHttpMessageHandler<OsuAuthHeaderHandler>();

        services.AddHttpClient<RedisSessionService>();
        services.AddSingleton<ISessionService, HybridSessionService>();
    }

    public static void AddHandlers(this IServiceCollection services)
    {
        services.AddAuthentication("Session")
            .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>("Session", null);

        services.AddTransient<OsuAuthHeaderHandler>();
    }

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IExamRepository, ExamRepository>();
        services.AddSingleton<IExamSessionRepository, ExamSessionRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
    }

    public static void AddUtilities(this IServiceCollection services)
    {
        services.AddOptions<SessionOptions>().BindConfiguration("Session");
        services.AddOptions<OsuOptions>().BindConfiguration("Osu");
        services.AddOptions<RedisOptions>().BindConfiguration("Redis");
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
    }
}
