namespace Core
{
    public class Renderer
    {
        private readonly Canvas canvas;
        private readonly RGBA[] imageFrame;
        private readonly RGBA[] canvasFrame;
        private readonly Dictionary<int, double> pixelDiffs;

        public Renderer(Canvas canvas, Image image)
        {
            this.canvas = canvas;
            imageFrame = ImageToFrame(image);
            canvasFrame = new RGBA[canvas.Width * canvas.Height];
            pixelDiffs = new Dictionary<int, double>();
        }

        public int GetImageCost()
        {
            CanvasToFrame();
            return ImageDiff();
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
        private void CanvasToFrame()
        {
            var blocks = canvas.Simplify();

            foreach (var block in blocks)
            {
                if (!block.HasRendered)
                {
                    block.HasRendered = true;
                    var frameTopLeft = new Point(block.BottomLeft.X, block.TopRight.Y);
                    var frameBottomRight = new Point(block.TopRight.X, block.BottomLeft.Y);

                    for (var y = frameBottomRight.Y; y < frameTopLeft.Y; y++)
                    {
                        for (var x = frameTopLeft.X; x < frameBottomRight.X; x++)
                        {
                            canvasFrame[y * canvas.Width + x] = block.Color;
                            pixelDiffs.Remove(y * canvas.Width + x);
                        }
                    }
                }
            }
        }

        // was SimilarityChecker.imageDiff
        private int ImageDiff()
        { 
            double diff = 0;
            double alpha = 0.005;
            for (int index = 0; index < imageFrame.Length; index++)
            {
                if (!pixelDiffs.ContainsKey(index))
                {
                    var p1 = imageFrame[index];
                    var p2 = canvasFrame[index];
                    pixelDiffs[index] = PixelDiff(p1, p2);
                }

                diff += pixelDiffs[index];
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
