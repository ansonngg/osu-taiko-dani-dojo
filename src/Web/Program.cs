using OsuTaikoDaniDojo.Web.Utility;

namespace OsuTaikoDaniDojo.Web;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddDatabase();
        builder.Services.AddControllers();
        builder.Services.AddServices();
        builder.Services.AddRepositories();
        builder.Services.AddUtilities();

        var app = builder.Build();
        app.UseMiddleware();
        app.MapControllers();
        app.Run();
    }
}
