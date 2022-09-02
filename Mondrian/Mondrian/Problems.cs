using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Numerics;

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
            int width = original.Width;
            int height = original.Height;

            PixelFormat format = PixelFormat.Format32bppArgb;
            int depth = System.Drawing.Image.GetPixelFormatSize(format) / 8; // Return size in bytes

            byte[] imageBytes = new byte[original.Width * original.Height * depth];

            BitmapData bmpData = original.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, format);
            Marshal.Copy(bmpData.Scan0, imageBytes, 0, bmpData.Stride * height);

            original.UnlockBits(bmpData);

            RGBA[,] pixels = new RGBA[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int baseIndex = (x + y * height) * 4;
                    // ARGB -> RGBA requires offsetting by 1, 2, 3, and then 0
                    pixels[x, y] = new RGBA(imageBytes[baseIndex + 1], imageBytes[baseIndex + 2], imageBytes[baseIndex + 3], imageBytes[baseIndex]);
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
