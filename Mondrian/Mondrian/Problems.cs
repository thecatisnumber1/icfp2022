using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace Mondrian
{
    public class Problems
    {
        private static string? problemsDir = null;

        public static Image GetProblem(int problemNum)
        {
            throw new NotImplementedException();
        }

        public static int ProblemCount()
        {
            throw new NotImplementedException();
        }

        public static string ProblemsDirectory()
        {
            string FindProblemsDirectory()
            {
                bool ContainsScores(string dirName)
                {
                    var dirs = Directory.GetDirectories(dirName);
                    return (from dir in dirs select Path.GetFileName(dir)).Contains("Problems");
                }

                string dir = ".";
                while (!ContainsScores(dir))
                {
                    dir = Path.Join(dir, "..");

                    if (dir.Length > 100)
                    {
                        throw new Exception("Couldn't find Problems");
                    }
                }

                return Path.Join(dir, "Problems");
            }

            return problemsDir ??= FindProblemsDirectory();
        }

        public static string LocationFor(string filename)
        {
            return Path.Join(ProblemsDirectory(), filename);
        }
    }
}
