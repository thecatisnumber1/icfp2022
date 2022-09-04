using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ColorOptimizer
    {
        public static List<RGBA?> ChooseColors(List<Rectangle> regions, Image target)
        {
            return (from r in regions select (RGBA?)target.AverageColor(r)).ToList();
        }

        public static List<RGBA?> ChooseColorsSlow(List<Rectangle> regions, Image target)
        {
            bool[,] seen = new bool[target.Width, target.Height];
            List<RGBA?> result = new(regions.Count);
            result.AddRange(Enumerable.Repeat<RGBA?>(null, regions.Count));

            for (int i = regions.Count - 1; i >= 0; i--)
            {
                Rectangle rec = regions[i];
                int area = 0;
                IntRGB sum = IntRGB.ZERO;
                for (int x = rec.Left; x < rec.Right; x++)
                {
                    for (int y = rec.Bottom; y < rec.Top; y++)
                    {
                        if (!seen[x, y])
                        {
                            seen[x, y] = true;
                            sum += target[x, y];
                            area++;
                        }
                    }
                }

                if (area > 0)
                {
                    result[i] = sum / area;
                }
            }

            return result;
        }

        public static (List<RGBA?> colors, int score) ChooseColorsLars(List<Point> corners, Point start, Image target)
        {
            return new();
        }
    }
}
