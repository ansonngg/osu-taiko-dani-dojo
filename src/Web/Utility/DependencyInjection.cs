using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Application.Utility;
using OsuTaikoDaniDojo.Infrastructure.Repository;
using OsuTaikoDaniDojo.Infrastructure.Service;
using OsuTaikoDaniDojo.Web.Middleware;
using Supabase;
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

    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(
            _ =>
            {
                var url = builder.Configuration["Database:Url"];

                if (string.IsNullOrEmpty(url))
                {
                    throw builder.ExceptionSince("Database url is null or empty.");
                }

                var key = builder.Configuration["Database:Key"];
                var client = new Client(url, key, new SupabaseOptions { AutoConnectRealtime = false });
                client.InitializeAsync().Wait();
                return client;
            });
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpClient<RedisSessionService>();
        services.AddHttpClient<IOsuAuthService, OsuAuthService>();
        services.AddSingleton<ISessionService, HybridSessionService>();
    }

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IExamRepository, ExamRepository>();
    }

    public static void UseMiddleware(this WebApplication app)
    {
        app.UseMiddleware<SessionMiddleware>();
    }
}
