using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using System.Drawing;

namespace Mondrian
{
    public class Problems
    {
        private static string? problemsDir = null;

        public static Core.Image GetProblem(int num)
        {
            return LoadFile(LocationFor($"{num}.png"));
        }

        public static Core.Image LoadFile(string path)
        {
            Bitmap original = new Bitmap(path);
            Bitmap clone = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(original, new System.Drawing.Rectangle(0, 0, clone.Width, clone.Height));
            }
            RGBA[,] pixels = new RGBA[original.Width, original.Height];
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    var p = clone.GetPixel(x, y);
                    pixels[x, y] = new RGBA(p.R, p.G, p.B, p.A);
                }
            }

            return new Core.Image(pixels);
        }

        public static int ProblemCount()
        {
            throw new NotImplementedException();
        }

        private static string ProblemsDirectory()
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

        private static string LocationFor(string filename)
        {
            return Path.Join(ProblemsDirectory(), filename);
        }
    }
}
