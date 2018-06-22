using System;

namespace Mcma.Server
{
    public class ConsoleLogger : ILogger
    {
        /// <summary>
        /// Logs to the console
        /// </summary>
        /// <param name="level"></param>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        private void Log(string level, string messageTemplate, params object[] parameters)
        {
            Console.WriteLine($"[{level}] {string.Format(messageTemplate, parameters)}");
        }

        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        public void Debug(string messageTemplate, params object[] parameters)
        {
            Log(nameof(Debug), messageTemplate, parameters);
        }

        /// <summary>
        /// Logs an info message
        /// </summary>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        public void Info(string messageTemplate, params object[] parameters)
        {
            Log(nameof(Info), messageTemplate, parameters);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        public void Warning(string messageTemplate, params object[] parameters)
        {
            Log(nameof(Warning), messageTemplate, parameters);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        public void Error(string messageTemplate, params object[] parameters)
        {
            Log(nameof(Error), messageTemplate, parameters);
        }
    }
}