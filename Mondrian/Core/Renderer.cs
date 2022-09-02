using System.Reflection;

namespace Core
{
    public static class Renderer
    {
        public static int GetImageCost(Canvas canvas, Image image)
        {
            var cFrame = CanvasToFrame(canvas);
            var iFrame = ImageToFrame(image);
            return ImageDiff(iFrame, cFrame);
        }

        private static RGBA[] ImageToFrame(Image image)
        {
            var frame = new RGBA[image.Width * image.Height];
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    frame[i * image.Width + j] = image[i, j];
                }
            }
            return frame;
        }

        // Was Painter.draw(canvas)
        private static RGBA[] CanvasToFrame(Canvas canvas)
        {
            var blocks = canvas.Simplify();
            var frame = new RGBA[canvas.Width * canvas.Height];
            int size = 0;

            foreach (var block in blocks)
            {
                var frameTopLeft = new Point(block.BottomLeft.X, canvas.Height - block.TopRight.Y);
                var frameBottomRight = new Point(block.TopRight.X, canvas.Height - block.BottomLeft.Y);
                size += (frameBottomRight.X - frameTopLeft.X) * (frameBottomRight.Y - frameTopLeft.Y);

                for (var y = frameTopLeft.Y; y < frameBottomRight.Y; y++)
                {
                    for (var x = frameTopLeft.X; x < frameBottomRight.X; x++)
                    {
                        frame[y * canvas.Width + x] = block.Color;
                    }
                }
            }

            return frame;
        }

        // was SimilarityChecker.imageDiff
        private static int ImageDiff(RGBA[] f1, RGBA[] f2)
        { 
            double diff = 0;
            double alpha = 0.005;
            for (int index = 0; index < f1.Length; index++)
            {
                var p1 = f1[index];
                var p2 = f2[index];
                diff += PixelDiff(p1, p2);
            }
            return (int)Math.Round(diff * alpha);
          }

        // was SimilarityChecker.pixelDiff
        private static double PixelDiff(RGBA p1, RGBA p2)
        {
            var rDist = (p1.R - p2.R) * (p1.R - p2.R);
            var gDist = (p1.G - p2.G) * (p1.G - p2.G);
            var bDist = (p1.B - p2.B) * (p1.B - p2.B);
            var aDist = (p1.A - p2.A) * (p1.A - p2.A);
            var distance = Math.Sqrt(rDist + gDist + bDist + aDist);
            return distance;
        }
    }
}
