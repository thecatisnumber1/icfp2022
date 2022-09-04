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
            if (start != Point.ORIGIN) throw new NotImplementedException();

            double pixelCost = 0.0;
            List<RGBA?> colors = new(corners.Count);
            colors.AddRange(Enumerable.Repeat<RGBA?>(null, corners.Count));

            List<Point> currEdge = new() { new Point(0, target.Height), new Point(target.Width, 0) };
            IntRGB prevColorSum = IntRGB.ZERO;
            int prevArea = 0;
            for (int i = corners.Count - 1; i >= 0; i--)
            {
                Point currCorner = corners[i];

                if (!EdgeNeedsUpdating(currEdge, currCorner)) continue;

                var prevEdge = currEdge;
                currEdge = UpdateEdge(currEdge, currCorner);

                var (newColorSum, newArea) = ComputeSum(currEdge, target);
                int deltaArea = newArea - prevArea;
                RGBA color = (newColorSum - prevColorSum) / deltaArea;
                colors[i] = color;

                prevColorSum = newColorSum;
                prevArea = newArea;

                pixelCost += ComputePixelDiffs(prevEdge, currEdge, color, target);
            }

            pixelCost += ComputePixelDiffs(currEdge, new List<Point> { new(target.Width, target.Height) }, new RGBA(255, 255, 255, 255), target);

            return (colors, (int)Math.Round(pixelCost * 0.005));
        }

        private static double ComputePixelDiffs(List<Point> prevEdge, List<Point> currEdge, RGBA color, Image img)
        {
            double sum = 0.0;
            int x = 0;
            foreach (var (y1, y2) in Enumerable.Zip(EdgeHeights(prevEdge), EdgeHeights(currEdge))) {
                for (int y = y1; y < y2; y++)
                {
                    RGBA actual = img[x, y];
                    sum += actual.Diff(color);
                }

                x++;
            }
            return sum;
        }

        private static IEnumerable<int> EdgeHeights(List<Point> edge)
        {
            int prevX = 0;
            foreach (Point p in edge)
            {
                for (; prevX < p.X; prevX++)
                {
                    yield return p.Y;
                }
            }
        }

        private static bool EdgeNeedsUpdating(List<Point> oldEdge, Point point)
        {
            foreach (Point p2 in oldEdge)
            {
                if (IsShadowed(p2, point))
                {
                    return false;
                }
            }

            return true;

        }

        private static List<Point> UpdateEdge(List<Point> oldEdge, Point point)
        {
            List<Point> newEdge = new();

            bool added = false;
            foreach (Point p in oldEdge)
            {
                if (!added && p.X >= point.X)
                {
                    added = true;
                    newEdge.Add(point);
                }

                if (!IsShadowed(point, p))
                {
                    newEdge.Add(p);
                }
            }
            return newEdge;
        }

        private static bool IsShadowed(Point shader, Point other)
        {
            return other.X <= shader.X && other.Y <= shader.Y;
        }

        private static (IntRGB ColorSum, int Area) ComputeSum(List<Point> points, Image img)
        {
            int prevX = 0;

            IntRGB colorSum = new();
            int area = 0;
            foreach (Point p in points)
            {
                Rectangle rect = new Rectangle(new(prevX, 0), p);
                colorSum += img.ColorSum(rect);
                area += rect.Area;
            }

            return (colorSum, area);
        }
    }
}
