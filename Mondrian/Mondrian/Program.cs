using Core;
using AI;

namespace Mondrian
{
    public class Program
    {
        static void Main(string[] args)
        {
            LoggerBase logger = new ConsoleLogger();
            Args a = Args.ParseArgs(args);
            Runner.Run(a, logger);
        }
    }
}