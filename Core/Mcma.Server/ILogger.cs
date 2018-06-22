namespace Mcma.Server
{
    public interface ILogger
    {
        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        void Debug(string messageTemplate, params object[] parameters);

        /// <summary>
        /// Logs an info message
        /// </summary>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        void Info(string messageTemplate, params object[] parameters);

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        void Warning(string messageTemplate, params object[] parameters);

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="messageTemplate"></param>
        /// <param name="parameters"></param>
        void Error(string messageTemplate, params object[] parameters);
    }
}
