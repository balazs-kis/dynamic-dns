using System.Text;

namespace DynamicDnsClient.Logging;

public class ConsoleLogger : ILogger
{
    private readonly List<string> LogList = new(25);
    
    public bool TraceEnabled { get; }
    public IReadOnlyCollection<string> Logs => LogList;

    public ConsoleLogger(bool traceEnabled)
    {
        TraceEnabled = traceEnabled;
    }

    public void LogTrace(string message)
    {
        if (TraceEnabled)
        {
            Log("TRC", ConsoleColor.Gray, message);
        }
    }

    public void LogInformation(string message) => Log("INF", ConsoleColor.Green, message);
    
    public void LogWarning(string message) => Log("WRN", ConsoleColor.Yellow, message);

    public void LogError(string message) => Log("ERR", ConsoleColor.Red, message);

    private void Log(string level, ConsoleColor levelColor, string message)
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
