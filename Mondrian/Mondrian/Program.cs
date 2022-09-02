using Core;
using AI;

namespace Mondrian
{
    public class Program
    {
        static void Main(string[] args)
        {
            LoggerBase logger = new ConsoleLogger();

            logger.LogMessage("Hello, World!");

            Problems.GetProblem(1);
            logger.LogMessage("It worked.");
        }

        public record Args(
            string? algorithm,
            string user,
            int minProblemNumber,
            int maxProblemNumber,
            AIArgs aiArgs);

        static Args ParseArgs(string[] args)
        {
            throw new NotImplementedException();
            /*
            string? algorithm = null;
            string? user = null;
            int minProblemNumber = 1;
            int maxProblemNumber = 15;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--algorithm":
                    case "-a":
                        algorithm = args[++i];
                        break;
                    case "--user":
                    case "-u":
                        user = args[++i];
                        break;
                    case "--min":
                        minSize = int.Parse(args[++i]);
                        break;
                    case "--max":
                        maxSize = int.Parse(args[++i]);
                        break;
                    case "--unwind":
                        unwindAmount = int.Parse(args[++i]);
                        break;
                    case "--initialalgorithm":
                        initialAlgorithm = args[++i];
                        break;
                    case "--beamwidth":
                    case "-w":
                        beamWidth = int.Parse(args[++i]);
                        break;
                    case "--lookahead":
                        lookAhead = int.Parse(args[++i]);
                        break;
                    default:
                        throw new ArgumentException($"Unrecognized argument: {args[i]}");
                }
            }

            if (user == null)
            {
                throw new ArgumentException("--user argument is required");
            }

            return new Args(
                algorithm,
                user,
                minSize,
                maxSize,
                unwindAmount,
                initialAlgorithm,
                beamWidth,
                lookAhead);*/
        }
    }
}