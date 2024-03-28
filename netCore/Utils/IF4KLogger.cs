namespace KillFallout4.Utils
{
    internal interface IF4KLogger
    {
        void Log(LogLevel level, Exception? ex, string? message);
        void Log(LogLevel level, string message);
        void LogError(Exception ex, string message);
        void LogError(string message);
        void LogInfo(string message);
        void LogVerbose(string message);
        void LogWarning(string message);
    }
}