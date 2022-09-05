using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI
{
    public class InitialPointPicker
    {
        private static readonly Random staticRand = new();

        public static List<Point> PickPoints(Image img, int numPoints, Random? rand = null)
        {
            List<double> detailSum = ComputeDetaiSum(img);

            rand ??= staticRand;
            List<Point> result = new(numPoints);

            while (result.Count < numPoints)
            {
                int index = PickRandom(detailSum, rand);
                Point p = new Point(index / img.Height, index % img.Height);
                if (p.X > 0 && p.Y > 0 && p.X < img.Width && p.Y < img.Height)
                {
                    result.Add(p);
                }
            }

            return result;
        }

        private static int PickRandom(List<double> weightSum, Random rand)
        {
            double targetSum = rand.NextDouble() * weightSum[^1];
            int searchResult = weightSum.BinarySearch(targetSum);

            if (searchResult >= 0)
                return searchResult;

            return ~searchResult - 1;
        }

        private static List<double> ComputeDetaiSum(Image img)
        {
            List<double> detailSum = new(img.Width * img.Height + 1);
            detailSum.Add(0.0);

            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    detailSum.Add(detailSum[^1] + DetailAt(img, x, y));
                }
            }

            return detailSum;
        }

        private static double DetailAt(Image img, int x, int y)
        {
            int xRight = Math.Min(x + 1, img.Width - 1);
            int xLeft = Math.Max(x - 1, 0);
            int deltaX = xRight - xLeft;

            int yUp = Math.Min(y + 1, img.Height - 1);
            int yDown = Math.Max(y - 1, 0);
            int deltaY = yUp - yDown;

            Point pRight = new Point(xRight, y);
            Point pLeft = new Point(xLeft, y);
            Point pUp = new Point(x, yUp);
            Point pDown = new Point(x, yDown);

            return Math.Sqrt(ColorDerivSq(img[pDown], img[pUp], deltaY) + ColorDerivSq(img[pRight], img[pLeft], deltaX));
        }

        private static double ColorDerivSq(RGBA c1, RGBA c2, int deltaPoint)
        {
            double root = c1.Diff(c2) / deltaPoint;
            return root * root;
        }
    }
}
