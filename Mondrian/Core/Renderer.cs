namespace Core
{
    public class Renderer
    {
        private readonly Canvas canvas;
        private readonly RGBA[] imageFrame;
        private readonly RGBA[] canvasFrame;
        private readonly HashSet<int> hasPixelDiff;

        public Renderer(Canvas canvas, Image image)
        {
            this.canvas = canvas;
            imageFrame = ImageToFrame(image);
            canvasFrame = new RGBA[canvas.Width * canvas.Height];
            hasPixelDiff = new HashSet<int>();
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
            int size = 0;

            foreach (var block in blocks)
            {
                if (!block.HasRendered)
                {
                    block.HasRendered = true;
                    var frameTopLeft = new Point(block.BottomLeft.X, canvas.Height - block.TopRight.Y);
                    var frameBottomRight = new Point(block.TopRight.X, canvas.Height - block.BottomLeft.Y);
                    size += (frameBottomRight.X - frameTopLeft.X) * (frameBottomRight.Y - frameTopLeft.Y);

                    for (var y = frameTopLeft.Y; y < frameBottomRight.Y; y++)
                    {
                        for (var x = frameTopLeft.X; x < frameBottomRight.X; x++)
                        {
                            canvasFrame[y * canvas.Width + x] = block.Color;
                            hasPixelDiff.Remove(y * canvas.Width + x);
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
                if (!hasPixelDiff.Contains(index))
                {
                    var p1 = imageFrame[index];
                    var p2 = canvasFrame[index];
                    diff += PixelDiff(p1, p2);
                    hasPixelDiff.Add(index);
                }
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
