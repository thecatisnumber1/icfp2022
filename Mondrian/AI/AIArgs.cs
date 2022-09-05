﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI
{
    public record AIArgs(int problemNum, int rotation, int numPoints, int limit)
    {
        public static AIArgs ParseArgs(string[] args)
        {
            int problemNum = -1;
            int rotation = 0;
            int numPoints = 200;
            int limit = 7;

            for (int i = 0; i < args.Length; i++)
            {
                int intArg() => int.Parse(args[++i]);

                switch (args[i])
                {
                    case "--rotation":
                    case "-r":
                        rotation = intArg();
                        break;
                    case "--problem":
                    case "-p":
                        problemNum = intArg();
                        break;
                    case "--points":
                        numPoints = intArg();
                        break;
                    case "--limit":
                    case "-l":
                        limit = intArg();
                        break;
                }
            }

            if (problemNum == -1)
            {
                throw new Exception("Need to specify a problem!");
            }

            return new AIArgs(problemNum, rotation, numPoints, limit);
        }
    }
}
