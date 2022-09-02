using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using AI;
using System.Diagnostics;

namespace Mondrian
{
    public class Runner
    {
        public static void Run(Args args, LoggerBase logger)
        {
            if (args.algorithm == null)
            {
                throw new Exception("Specify algorithm to run.");
            }

            var solver = Solvers.GetSolver(args.algorithm);
            for (int problemNum = args.minProblemNumber; problemNum <= args.maxProblemNumber; problemNum++)
            {
                Stopwatch watch = Stopwatch.StartNew();
                logger.LogMessage($"Considering problem #{problemNum}");
                Image problem = Problems.GetProblem(problemNum);
                Picasso picasso = new Picasso(problem);
                solver(picasso, args.aiArgs, logger);
                logger.LogMessage($"Score = {picasso.Score}, elapsed = {watch.Elapsed}");
                // TODO: Need to add printing the solution that was found.
            }
        }
    }
}
