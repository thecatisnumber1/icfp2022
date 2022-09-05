using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI
{
    public static class AllCutsAI
    {
        private static LoggerBase Logger;
        private static Point MinRes = new Point(5, 5);

        private static int ShortCutCount = 0;

        public static void Solve(Core.Picasso picasso, AIArgs args, LoggerBase logger)
        {
            Logger = logger;

            foreach (SimpleBlock sb in picasso.AllSimpleBlocks.ToList())
            {
                Recurse(picasso, sb);
            }
        }

        private static void Recurse(Core.Picasso picasso, SimpleBlock block)
        {
            RGBA targetColor = picasso.AverageTargetColor(block);

            if (block.Size.X <= MinRes.X && block.Size.Y <= MinRes.Y)
            {
                picasso.Color(block.ID, targetColor);
                Logger.Render(picasso);
                return;
            }

            // Compute stdev of colors in the block from their target
            (double r, double g, double b, double a) = StandardDeviation(block, targetColor);

            // All 0's means a solid block
            double tolerance = 5;
            if (r <= tolerance && g <= tolerance && b <= tolerance && a <= tolerance)
            {
                picasso.Color(block.ID, targetColor);
                Logger.Render(picasso);
                Logger.LogStatusMessage($"Shortcut {++ShortCutCount}");
                return;
            }

            if (targetColor != block.Color)
            {
                // Get center point of block for cut
                Point blockCenter = new Point(block.BottomLeft.X + block.Size.X / 2, block.BottomLeft.Y + block.Size.Y / 2);
                // Split in 4 and recurse.
                List<SimpleBlock> blocks = picasso.PointCut(block.ID, blockCenter).Cast<SimpleBlock>().ToList();

                foreach(var subBlock in blocks)
                {
                    Recurse(picasso, subBlock);
                }
            }
        }

        private static (double r, double g, double b, double a) StandardDeviation(SimpleBlock block, RGBA average)
        {
            double r = 0;
            double g = 0;
            double b = 0;
            double a = 0;

            for (int x = 0; x < block.Size.X; x++)
            {
                for (int y = 0; y < block.Size.Y; y++)
                {
                    RGBA pointColor = block.Image == null ? block.GetColorAt(x, y) : block.GetColorAt(block.BottomLeft.X + x, block.BottomLeft.Y + y);
                    r += Math.Pow(pointColor.R - average.R, 2);
                    g += Math.Pow(pointColor.G - average.G, 2);
                    b += Math.Pow(pointColor.B - average.B, 2);
                    a += Math.Pow(pointColor.A - average.A, 2);
                }
            }
            int sampleSize = (block.Size.X * block.Size.Y);
            r = Math.Sqrt(r / sampleSize);
            g = Math.Sqrt(g / sampleSize);
            b = Math.Sqrt(b / sampleSize);
            a = Math.Sqrt(a / sampleSize);

            return (r, g, b, a);
        }

    }
}
