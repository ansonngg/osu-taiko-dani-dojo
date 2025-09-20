namespace Application.Utility;

public static class LoggingHelper
{
    public static void Log(this object obj, string message) => Console.WriteLine($"[{obj.GetType().Name}] {message}");
    public static Exception ExceptionSince(this object obj, string message) => new($"[{obj.GetType().Name}] {message}");
}
