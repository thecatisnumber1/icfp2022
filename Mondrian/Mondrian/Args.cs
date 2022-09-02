using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AI;

namespace Mondrian
{
    public record Args(
            string? algorithm,
            string user,
            int minProblemNumber,
            int maxProblemNumber,
            AIArgs aiArgs)
    {
        public static Args ParseArgs(string[] args)
        {
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
                        minProblemNumber = int.Parse(args[++i]);
                        break;
                    case "--max":
                        maxProblemNumber = int.Parse(args[++i]);
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
                minProblemNumber,
                maxProblemNumber,
                new AIArgs());
        }
    }
}
