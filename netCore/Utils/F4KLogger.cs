using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KillFallout4.Utils
{
    public enum LogLevel
    {
        Verbose = 0,
        Information = 1,
        Warning = 2,
        Error = 3
    }

    internal class F4KLogger : IF4KLogger
    {
        private static string _logFolder;

        public string LogFolder => _logFolder;

        public string PathToCurrentLogFile { get; protected set; }

        public LogLevel LogLevel { get; set; }

        public F4KLogger() : this (LogLevel.Information) { }

        public F4KLogger(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }


        public void LogError(string message) { Log(LogLevel.Error, message); }
        public void LogError(Exception ex, string message) { Log(LogLevel.Error, ex, message); }
        public void LogWarning(string message) { Log(LogLevel.Warning, message); }
        public void LogInfo(string message) { Log(LogLevel.Information, message); }
        public void LogVerbose(string message) { Log(LogLevel.Verbose, message); }

        public void Log(string message) { Log(this.LogLevel, null, message); }

        public void Log(Exception? ex, string? message) { Log(this.LogLevel, ex, message); }

        public void Log(LogLevel level, string message) { Log(level, null, message); }


        public void Log(LogLevel level, Exception? ex, string? message)
        {
            var logDateTime = DateTime.Now;
            var levelMsg = level.ToString();
            var logTime = logDateTime.ToString("MM/dd/yyyy HH:mm:ss.fff");
            var trimmedMsg = (message ?? string.Empty).Trim('\n', '\r');

            var logErrMsg = string.Empty;
            if (ex != null)
            {
                logErrMsg = BuildErrorMessage(ex);
            }

            var outMsg = $"{logTime},{levelMsg},\"{trimmedMsg}\",\"{logErrMsg}\"";

            PathToCurrentLogFile = BuildLogFile(logDateTime);

            File.AppendAllText(PathToCurrentLogFile, outMsg + Environment.NewLine);
        }


        private static string BuildLogFile(DateTime logDateTime)
        {
            var logFileName = logDateTime.ToString("yyyyMMdd") + "_killFallout4.log";

            var _logFilePath = Path.Combine(_logFolder, logFileName);

            return _logFilePath;
        }


        private static string BuildErrorMessage(Exception? ex)
        {
            if (ex == null) { return string.Empty; }

            var result = new StringBuilder();
            var indent = "";
            var innerEx = ex;
            while (innerEx != null)
            {
                var exType = innerEx.GetType().Name;
                result.AppendLine($"{indent}{exType}: {innerEx.Message}");

                indent += "   ";
                innerEx = innerEx.InnerException;
            }

            result.AppendLine(ex.StackTrace);

            return result.ToString().Trim('\n', '\r'); ;
        }


        static F4KLogger()
        {
            var asm = Assembly.GetExecutingAssembly();

            var info = new FileInfo(asm.Location);

            if (info.Directory != null)
            {
                var baseFolder = info.Directory.FullName;

                _logFolder = Path.Combine(baseFolder, "logs");

                if (!Directory.Exists(_logFolder)) { Directory.CreateDirectory(_logFolder); }
            }

            _logFolder = _logFolder ?? string.Empty;
        }
    }
}
