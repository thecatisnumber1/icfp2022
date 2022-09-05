﻿namespace Core
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

        private static Dictionary<int, Dictionary<int, Dictionary<int, double>>> nestedLarsCache = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();

        public static void ClearLarsCache()
        {
            nestedLarsCache.Clear();
        }

        private static int HashEdge(List<Point> edge)
        {
            var currHash = new HashCode();
            edge.ForEach(x => currHash.Add(x));
            return currHash.ToHashCode();
        }

        public static (List<RGBA?> colors, int score) ChooseColorsLars(List<Point> corners, Point start, Image target)
        {
            if (start != Point.ORIGIN) throw new NotImplementedException();

            double pixelCost = 0.0;
            List<RGBA?> colors = new(corners.Count);
            colors.AddRange(Enumerable.Repeat<RGBA?>(null, corners.Count));

            List<Point> currEdge = new() { new Point(0, target.Height), new Point(target.Width, 0) };
            var currHash = HashEdge(currEdge);
            IntRGB prevColorSum = IntRGB.ZERO;
            int prevArea = 0;
            for (int i = corners.Count - 1; i >= 0; i--)
            {
                Point currCorner = corners[i];

                if (!EdgeNeedsUpdating(currEdge, currCorner)) continue;

                var prevEdge = currEdge;
                var prevHash = currHash;
                currEdge = UpdateEdge(currEdge, currCorner);
                currHash = HashEdge(currEdge);

                var (newColorSum, newArea) = ComputeSum(currEdge, target);
                int deltaArea = newArea - prevArea;
                RGBA color = (newColorSum - prevColorSum) / deltaArea;
                colors[i] = color;

                prevColorSum = newColorSum;
                prevArea = newArea;

                // Awful Larse caching code
                if (!nestedLarsCache.ContainsKey(color.GetHashCode()))
                {
                    nestedLarsCache[color.GetHashCode()] = new Dictionary<int, Dictionary<int, double>>();
                }
                if (!nestedLarsCache[color.GetHashCode()].ContainsKey(prevHash))
                {
                    nestedLarsCache[color.GetHashCode()][prevHash] = new Dictionary<int, double>();
                }
                if (!nestedLarsCache[color.GetHashCode()][prevHash].ContainsKey(currHash))
                {
                    nestedLarsCache[color.GetHashCode()][prevHash][currHash] = FastComputePixelDiffs(prevEdge, currEdge, color, target);
                }

                var diff = nestedLarsCache[color.GetHashCode()][prevHash][currHash];
                pixelCost += diff;
            }

            pixelCost += FastComputePixelDiffs(currEdge, new List<Point> { new(target.Width, target.Height) }, new RGBA(255, 255, 255, 255), target);

            return (colors, (int)Math.Round(pixelCost * 0.005));
        }

        private static double FastComputePixelDiffs(List<Point> prevEdge, List<Point> currEdge, RGBA color, Image img)
        {
            int prevIndex = 0;
            int currIndex = 0;
            double sum = 0;
            for (int x = 0; x < img.Width; x++)
            {
                while (prevEdge[prevIndex].X <= x)
                {
                    prevIndex++;
                }

                while (currEdge[currIndex].X <= x)
                {
                    currIndex++;
                }

                for (int y = prevEdge[prevIndex].Y; y < currEdge[currIndex].Y; y++)
                {
                    RGBA actual = img[x, y];
                    sum += actual.Diff(color);
                }
            }

            return sum;
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
                prevX = p.X;
            }

            return (colorSum, area);
        }
    }
}
