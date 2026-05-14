using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Source.Shared.Services
{
    public interface ILoggingService
    {
        void Info(string message, params object[] args);
        void Warning(string message, params object[] args);
        void Error(System.Exception ex, string message, params object[] args);
        void Debug(Object context, string message);
        void Dispose();
    }

    public class LoggingService : ILoggingService
    {
        private readonly Serilog.ILogger _logger;

        public LoggingService()
        {
#if UNITY_EDITOR
            var projectDirectory = Path.GetDirectoryName(UnityEngine.Application.dataPath) ?? "";
            var logDirectoryPath = Path.Combine(projectDirectory, "Logs", "app_log.txt");
#else
            string logDirectoryPath = Path.Combine(Application.persistentDataPath, "Logs", "app_log.txt");
#endif
            var consoleFormatter = new MessageTemplateTextFormatter("[{Level:u3}] {Message:lj}{NewLine}{Exception}");

            _logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithProperty("Device", SystemInfo.deviceModel)
                .Enrich.WithProperty("OS", SystemInfo.operatingSystem)
                .Enrich.WithProperty("AppVersion", UnityEngine.Application.version)
                .Enrich.FromLogContext()
                .WriteTo.Async(a => a.File(logDirectoryPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    flushToDiskInterval: System.TimeSpan.FromSeconds(2),
                    outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"))
                .WriteTo.Sink(new UnitySink(consoleFormatter, UnityEngine.Debug.unityLogger))
                .CreateLogger();

            Log.Logger = _logger;

            Info("Log System Initialized at {Path}", logDirectoryPath);
        }

        [HideInCallstack]
        public void Info(string message, params object[] args) =>
            _logger.Information(message, args);

        [HideInCallstack]
        public void Warning(string message, params object[] args) =>
            _logger.Warning(message, args);

        [HideInCallstack]
        public void Error(System.Exception ex, string message, params object[] args) =>
            _logger.Error(ex, message, args);

        [HideInCallstack]
        public void Debug(Object context, string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _logger.ForContext("UnityContext", context.name).Information(message);
#endif
        }

        public void Dispose() =>
            Log.CloseAndFlush();
    }

    public sealed class UnitySink : ILogEventSink
    {
        private readonly ITextFormatter _formatter;

        private readonly UnityEngine.ILogger _unityLogger;

        public UnitySink(ITextFormatter formatter, UnityEngine.ILogger unityLogger)
        {
            _formatter = formatter;
            _unityLogger = unityLogger;
        }

        [HideInCallstack]
        public void Emit(LogEvent logEvent)
        {
            using var buffer = new StringWriter();

            _formatter.Format(logEvent, buffer);

            var logType = logEvent.Level switch
            {
                LogEventLevel.Verbose or LogEventLevel.Debug or LogEventLevel.Information => LogType.Log,
                LogEventLevel.Warning => LogType.Warning,
                LogEventLevel.Error or LogEventLevel.Fatal => LogType.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(logEvent.Level), "Unknown log level"),
            };

            object message = buffer.ToString().Trim();

            _unityLogger.Log(logType, message);
        }
    }
}