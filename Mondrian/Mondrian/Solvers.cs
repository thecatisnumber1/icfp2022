using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using AI;

namespace Mondrian
{
    public class Solvers
    {
        public delegate void Solver(Core.Picasso picasso, AIArgs args, LoggerBase logger);


        private static Dictionary<string, Solver> solvers = new()
        {
            ["Checkerboard"] = CheckerboardAI.Solve,
            ["AllCuts"] = AllCutsAI.Solve,
        };

        public static Solver GetSolver(string algorithm)
        {
            return solvers[algorithm];
        }

        public static string[] Names()
        {
            return solvers.Keys.ToArray();
        }
    }
}
