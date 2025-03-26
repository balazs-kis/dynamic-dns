namespace DynamicDnsClient.Logging;

public interface ILogger
{
    bool TraceEnabled { get; }
    IReadOnlyCollection<string> Logs { get; }

    void LogTrace(string message);
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(string message);
}