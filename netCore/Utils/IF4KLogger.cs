using Microsoft.Extensions.Logging;

namespace KillFallout4.Utils
{
    internal interface IF4KLogger: ILogger
    {
        string PathToCurrentLogFile { get; }
        LogLevel LogLevel { get; set; }
        void Log(LogLevel level, Exception? ex, string? message);
        void Log(LogLevel level, string message);
        void LogError(Exception ex, string message);
        void LogError(string message);
        void LogInfo(string message);
        void LogVerbose(string message);
        void LogTrace(string message);
        void LogWarning(string message);
    }
}