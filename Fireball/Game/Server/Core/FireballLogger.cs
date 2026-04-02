using Microsoft.Extensions.Logging;

namespace Fireball.Fireworks
{
    public interface IFireballLogger
    {
        void Log(string message, LogLevel logLevel = LogLevel.Information);
        void LogDebug(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogCritical(string message);
    }

    public class FireballLogger : IFireballLogger
    {
        private readonly string _module;
        private readonly ILogger _logger;

        public static LogLevel LogLevel = LogLevel.Information;

        public FireballLogger(string moduleName, ILogger logger)
        {
            _module = moduleName;
            _logger = logger;
        }

        public void Log(string message, LogLevel logLevel = LogLevel.Information)
        {
            if (logLevel <= LogLevel.Debug) LogDebug(message);
            else if (logLevel == LogLevel.Information) LogInfo(message);
            else if (logLevel == LogLevel.Warning) LogWarning(message);
            else if (logLevel == LogLevel.Error) LogError(message);
            else if (logLevel == LogLevel.Critical) LogCritical(message);
        }

        public void LogDebug(string message)
        {
            if (LogLevel <= LogLevel.Debug) System.Console.WriteLine($"[{_module}] {message}"); //_logger.LogInformation($"[{_module}] {message}");
        }

        public void LogInfo(string message)
        {
            if (LogLevel <= LogLevel.Information) _logger.LogInformation($"[{_module}] {message}");
        }

        public void LogWarning(string message)
        {
            if (LogLevel <= LogLevel.Warning) _logger.LogWarning($"[{_module}] {message}"); //\n(ActionId = {FireballServer.ActionId ?? "null"}, GameSession = {FireballServer.GameSession ?? "null"})");
        }

        public void LogError(string message)
        {
            if (LogLevel <= LogLevel.Error) _logger.LogError($"[{_module}] {message}"); //\n(ActionId = {FireballServer.ActionId ?? "null"}, GameSession = {FireballServer.GameSession ?? "null"})");
        }

        public void LogCritical(string message)
        {
            if (LogLevel <= LogLevel.Critical) _logger.LogCritical($"[{_module}] {message}"); //\n(ActionId = {FireballServer.ActionId ?? "null"}, GameSession = {FireballServer.GameSession ?? "null"})");
        }
    }
}
