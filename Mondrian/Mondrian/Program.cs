using Core;

namespace Mondrian
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LoggerBase logger = new ConsoleLogger();

            logger.LogMessage("Hello, World!");
        }
    }
}