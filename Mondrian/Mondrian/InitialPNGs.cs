using Core;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Mondrian
{
    public class InitialPNGs
    {
        private static string? pngDir = null;
        private static int? pngCount = null;

        public static Core.Image? GetInitialPNG(int num)
        {
            return LoadFile(LocationFor($"{num}.source.png"));
        }

        public static Core.Image? LoadFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

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
                    pixels[x, height - 1 - y] = new RGBA(imageBytes[baseIndex + 2], imageBytes[baseIndex + 1], imageBytes[baseIndex], imageBytes[baseIndex + 3]);
                }
            }

            return new Core.Image(pixels);
        }

        public static int PNGCount()
        {
            return pngCount ??= Directory.GetFiles(PNGDirectory()).Length;
        }

        private static string PNGDirectory()
        {
            string FindPngDirectory()
            {
                bool ContainsScores(string dirName)
                {
                    var dirs = Directory.GetDirectories(dirName);
                    return (from dir in dirs select Path.GetFileName(dir)).Contains("InitialPNGs");
                }

                string dir = ".";
                while (!ContainsScores(dir))
                {
                    dir = Path.Join(dir, "..");

                    if (dir.Length > 100)
                    {
                        throw new Exception("Couldn't find Initial PNGs");
                    }
                }

                return Path.Join(dir, "InitialPNGs");
            }

            return pngDir ??= FindPngDirectory();
        }

        private static string LocationFor(string filename)
        {
            return Path.Join(PNGDirectory(), filename);
        }
    }
}
