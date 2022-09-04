namespace Core
{
    public abstract class LoggerBase
    {
        public List<Rectangle> UserSelectedRectangles = new List<Rectangle>();

        public abstract void Render(Picasso image);

        /// <summary>
        /// Pauses immediately until the logger returns
        /// </summary>
        /// <param name="immediate">If true, blocks in the Logger. Else blocks opportunistically.</param>
        /// <remarks>If false, the visualizer will only block if the user has selected the Pause button.</remarks>
        public abstract void Break(bool immediate = false);

        public abstract void LogMessage(string logString);

        public abstract void LogError(string logString);
    }

    public class ConsoleLogger : LoggerBase
    {
        public override void Render(Picasso image)
        {
            // Do nothing for now.
        }

        public override void Break(bool immediate)
        {
            // Do nothing;
        }

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