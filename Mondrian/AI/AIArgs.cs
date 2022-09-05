using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI
{
    public record AIArgs(int problemNum, int rotation, int numPoints)
    {
        public static AIArgs ParseArgs(string[] args)
        {
            int problemNum = -1;
            int rotation = 0;
            int numPoints = 200;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--rotation":
                    case "-r":
                        rotation = int.Parse(args[++i]);
                        break;
                    case "--problem":
                    case "-p":
                        problemNum = int.Parse(args[++i]);
                        break;
                    case "--points":
                        numPoints = int.Parse(args[++i]);
                        break;
                }
            }

            if (problemNum == -1)
            {
                throw new Exception("Need to specify a problem!");
            }

            return new AIArgs(problemNum, rotation, numPoints);
        }
    }
}
