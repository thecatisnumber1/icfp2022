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

            if (args.submit)
            {
                Rest.CacheBests();
            }

            var solver = Solvers.GetSolver(args.algorithm);
            for (int problemNum = args.minProblemNumber; problemNum <= args.maxProblemNumber; problemNum++)
            {
                Stopwatch watch = Stopwatch.StartNew();
                logger.LogMessage($"Considering problem #{problemNum}");
                Image problem = Problems.GetProblem(problemNum);
                InitialConfig? initialConfig = InitialConfigs.GetInitialConfig(problemNum);
                Core.Picasso picasso = new Picasso(problem, initialConfig);
                solver(picasso, args.aiArgs, logger);
                logger.LogMessage($"Score = {picasso.Score}, instructionCost = {(picasso.TotalInstructionCost / ((double)picasso.Score)).ToString("0.00")} = {watch.Elapsed}");
                List<string> instructions = picasso.SerializeInstructions();
                File.WriteAllLines($"{problemNum}.sol", instructions);

                if (args.submit)
                {
                    Rest.Upload(problemNum, String.Join("\n", instructions), picasso.Score);
                }
            }
        }
    }
}
