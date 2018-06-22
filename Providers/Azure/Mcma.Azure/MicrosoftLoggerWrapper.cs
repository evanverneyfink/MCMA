using Microsoft.Extensions.Logging;
using ILogger = Mcma.Server.ILogger;

namespace Mcma.Azure
{
    public class MicrosoftLoggerWrapper : ILogger
    {
        /// <summary>
        /// Instantiates a <see cref="MicrosoftLoggerWrapper"/>
        /// </summary>
        /// <param name="loggerFactory"></param>
        public MicrosoftLoggerWrapper(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger("Function.MicrosoftLoggerWrapper.User");
        }

        /// <summary>
        /// Gets the logger
        /// </summary>
        private Microsoft.Extensions.Logging.ILogger Logger { get; }

        /// <summary>
        /// Logs a message using the specified log level
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        private void Log(LogLevel logLevel, string messageTemplate, params object[] parameters) => Logger.Log(logLevel, new EventId(0), messageTemplate, parameters);

        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        public void Debug(string messageTemplate, params object[] parameters) => Log(LogLevel.Debug, messageTemplate, parameters);

        /// <summary>
        /// Logs an info message
        /// </summary>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        public void Info(string messageTemplate, params object[] parameters) => Log(LogLevel.Information, messageTemplate, parameters);

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        public void Warning(string messageTemplate, params object[] parameters) => Log(LogLevel.Warning, messageTemplate, parameters);

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        public void Error(string messageTemplate, params object[] parameters) => Log(LogLevel.Error, messageTemplate, parameters);
    }
}