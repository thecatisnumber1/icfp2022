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

        public static bool DISABLE_LARSE_CACHE = false;

        private static Dictionary<RGBA, Dictionary<int, Dictionary<int, double>>> nestedLarsCache = new Dictionary<RGBA, Dictionary<int, Dictionary<int, double>>>();

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

        public static (List<RGBA?> colors, int score) ChooseColorsLars(List<Point> corners, Point start, Image target, bool fast)
        {
            if (start != Point.ORIGIN) throw new NotImplementedException();

            HistogramMedian[] meds = new HistogramMedian[] { new(), new(), new() };

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

                RGBA color;
                if (fast)
                {
                    var (newColorSum, newArea) = ComputeSum(currEdge, target);
                    int deltaArea = newArea - prevArea;
                    color = (newColorSum - prevColorSum) / deltaArea;
                    colors[i] = color;

                    prevColorSum = newColorSum;
                    prevArea = newArea;
                }
                else
                {
                    color = GetMedianColor(prevEdge, currEdge, target, meds);
                    colors[i] = color;
                }


                double diff;
                if (DISABLE_LARSE_CACHE)
                {
                    diff = FastComputePixelDiffs(prevEdge, currEdge, color, target);
                }
                else
                {
                    // Awful Larse caching code
                    if (!nestedLarsCache.ContainsKey(color))
                    {
                        nestedLarsCache[color] = new Dictionary<int, Dictionary<int, double>>();
                    }
                    if (!nestedLarsCache[color].ContainsKey(prevHash))
                    {
                        nestedLarsCache[color][prevHash] = new Dictionary<int, double>();
                    }
                    if (!nestedLarsCache[color][prevHash].ContainsKey(currHash))
                    {
                        nestedLarsCache[color][prevHash][currHash] = FastComputePixelDiffs(prevEdge, currEdge, color, target);
                    }
                    diff = nestedLarsCache[color][prevHash][currHash];
                }

                pixelCost += diff;
            }

            pixelCost += FastComputePixelDiffs(currEdge, new List<Point> { new(target.Width, target.Height) }, new RGBA(255, 255, 255, 255), target);

            return (colors, (int)Math.Round(pixelCost * 0.005));
        }

        private static RGBA GetMedianColor(List<Point> prevEdge, List<Point> currEdge, Image img, HistogramMedian[] meds)
        {
            /*
            List<int> rPixels = new List<int>();
            List<int> gPixels = new List<int>();
            List<int> bPixels = new List<int>();
            List<int> aPixels = new List<int>();
            */
            var rPixels = meds[0];
            var bPixels = meds[1];
            var gPixels = meds[2];

            int prevIndex = 0;
            int currIndex = 0;
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
                    rPixels.Add(img[x, y].R);
                    gPixels.Add(img[x, y].G);
                    bPixels.Add(img[x, y].B);
                    //aPixels.Add(img[x, y].A);
                }
            }

            //int GetMedian(List<int> pixels)
            int GetMedian(HistogramMedian pixels)
            {
                //pixels.Sort();
                //return pixels[pixels.Count / 2];

                //return FindMedian.Median(pixels);

                return pixels.GetMedianAndClear();
            }

            return new RGBA(GetMedian(rPixels), GetMedian(gPixels), GetMedian(bPixels), 255);

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
