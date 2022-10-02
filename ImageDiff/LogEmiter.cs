namespace ImageDiff
{
    internal class LogEmiter
    {
        public static EventHandler<LogEventArgs> LoggingEvent;

        public static void LogMessage(string message) {
            LoggingEvent?.Invoke(null, new LogEventArgs(message));
    }

    }

    public class LogEventArgs: EventArgs
    {
        public string Message { get; set; }

        public LogEventArgs(string message)
        {
            Message = message;
        }
    }
}
