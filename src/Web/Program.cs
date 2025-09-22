using OsuTaikoDaniDojo.Web.Utility;

namespace OsuTaikoDaniDojo.Web;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddOptions();
        builder.Services.AddServices();
        builder.Services.AddControllers();

        var app = builder.Build();
        app.UseMiddleware();
        app.MapControllers();
        app.Run();
    }
}
