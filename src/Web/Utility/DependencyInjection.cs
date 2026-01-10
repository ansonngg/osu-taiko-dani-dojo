using Microsoft.AspNetCore.Authentication;
using OsuTaikoDaniDojo.Application.Interface;
using OsuTaikoDaniDojo.Application.Options;
using OsuTaikoDaniDojo.Infrastructure.Repository;
using OsuTaikoDaniDojo.Infrastructure.Service;
using OsuTaikoDaniDojo.Web.Const;
using OsuTaikoDaniDojo.Web.Handler;
using Supabase;

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

    extension(IServiceCollection services)
    {
        public void AddServices()
        {
            services.AddHttpClient<IOsuAuthService, OsuAuthService>(
                client => { client.BaseAddress = new Uri("https://osu.ppy.sh"); });

            services.AddHttpClient<IOsuMultiplayerRoomService, OsuMultiplayerRoomService>(
                    client => { client.BaseAddress = new Uri("https://osu.ppy.sh/api/v2/rooms/"); })
                .AddHttpMessageHandler<OsuAuthHeaderHandler>();

            services.AddHttpClient<RedisSessionService>();
            services.AddSingleton<ISessionService, HybridSessionService>();
        }

        public void AddHandlers()
        {
            services.AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = AppDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = AppDefaults.AuthenticationScheme;
                    })
                .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(
                    AppDefaults.AuthenticationScheme,
                    null);

            services.AddTransient<OsuAuthHeaderHandler>();
        }

        public void AddRepositories()
        {
            services.AddSingleton<IExamRepository, ExamRepository>();
            services.AddSingleton<IExamSessionRepository, ExamSessionRepository>();
            services.AddSingleton<IGradeCertificateRepository, GradeCertificateRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
        }

        public void AddUtilities()
        {
            services.AddOptions<LoginSessionOptions>().BindConfiguration("LoginSession");
            services.AddOptions<OsuOptions>().BindConfiguration("Osu");
            services.AddOptions<RedisOptions>().BindConfiguration("Redis");
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
        }
    }
}
