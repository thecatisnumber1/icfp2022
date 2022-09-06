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
            bool submit,
            AIArgs aiArgs)
    {
        public static Args ParseArgs(string[] args)
        {
            string? algorithm = null;
            string user = "";
            int minProblemNumber = 1;
            int maxProblemNumber = Problems.ProblemCount();
            bool submit = false;

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
                    case "--submit":
                        submit = true;
                        break;
                }
            }

            return new Args(
                algorithm,
                user,
                minProblemNumber,
                maxProblemNumber,
                submit,
                AIArgs.ParseArgs(args));
        }
    }
}
