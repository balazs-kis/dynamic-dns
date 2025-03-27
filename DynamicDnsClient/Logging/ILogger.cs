namespace DynamicDnsClient.Logging;

public interface ILogger
{
    bool TraceEnabled { get; }
    IList<string>? Logs { get; set; }

    void LogTrace(string message);
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(string message);
}