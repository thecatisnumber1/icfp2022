namespace Core
{
    public abstract class LoggerBase
    {
        public abstract void LogMessage(string logString);

        public abstract void LogError(string logString);
    }

    public class ConsoleLogger : LoggerBase
    {
        public override void LogMessage(string logString)
        {
            Console.WriteLine(logString);
        }

        public override void LogError(string logString)
        {
            Console.Error.WriteLine(logString);
        }
    }
}