using System.Text;

namespace DynamicDnsClient.Logging;

public static class ConsoleLogger
{
    private static readonly List<string> LogList = new(25);
    
    public static bool TraceEnabled { get; set; } = true;
    public static IReadOnlyCollection<string> Logs => LogList;

    public static void LogTrace(string message)
    {
        if (TraceEnabled)
        {
            Log("TRC", ConsoleColor.Gray, message);
        }
    }

    public static void LogInformation(string message) => Log("INF", ConsoleColor.Green, message);
    
    public static void LogWarning(string message) => Log("WRN", ConsoleColor.Yellow, message);

    public static void LogError(string message) => Log("ERR", ConsoleColor.Red, message);

    private static void Log(string level, ConsoleColor levelColor, string message)
    {
        const string timeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        
        var messageBuilder = new StringBuilder();
        var timePart = $"{DateTime.Now.ToString(timeFormat)}  ";
        var levelPart = $"[{level}]";
        var messagePart = $"  {message}";
        
        var originalColor = Console.ForegroundColor;
        
        messageBuilder.Append(timePart);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(timePart);
        
        messageBuilder.Append(levelPart);
        Console.ForegroundColor = levelColor;
        Console.Write(levelPart);
        
        messageBuilder.Append(messagePart);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(messagePart);

        Console.ForegroundColor = originalColor;
        
        LogList.Add(messageBuilder.ToString());
    }
}
