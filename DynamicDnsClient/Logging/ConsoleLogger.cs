namespace DynamicDnsClient.Logging;

public static class ConsoleLogger
{
    public static bool TraceEnabled { get; set; } = true;

    public static void LogTrace(string message)
    {
        if (TraceEnabled)
        {
            Log("TRACE", ConsoleColor.Gray, message);
        }
    }

    public static void LogInformation(string message) => Log("INFO", ConsoleColor.Green, message);
    
    public static void LogWarning(string message) => Log("WARN", ConsoleColor.Yellow, message);

    public static void LogError(string message) => Log("ERROR", ConsoleColor.Red, message);

    private static void Log(string level, ConsoleColor levelColor, string message)
    {
        const string timeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        var originalColor = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{DateTime.Now.ToString(timeFormat)}  ");
        
        Console.ForegroundColor = levelColor;
        Console.Write($"[{level}]".PadRight(7));
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"  {message}");

        Console.ForegroundColor = originalColor;
    }
}
