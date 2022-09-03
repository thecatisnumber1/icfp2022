namespace Core
{
    public class Renderer
    {
        private readonly Canvas canvas;
        private readonly Image image;
        private readonly RGBA[,] renderCanvas;
        private readonly double[,] pixelDiffs;

        public Renderer(Canvas canvas, Image image)
        {
            this.canvas = canvas;
            this.image = image;
            renderCanvas = new RGBA[canvas.Width, canvas.Height];
            pixelDiffs = new double[canvas.Width, canvas.Height];
        }

        public int GetImageCost()
        {
            CanvasToFrame();
            return ImageDiff();
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

                    for (var y = block.BottomLeft.Y; y < block.TopRight.Y; y++)
                    {
                        for (var x = block.BottomLeft.X; x < block.TopRight.X; x++)
                        {
                            if (renderCanvas[x, y] != block.Color)
                            {
                                renderCanvas[x, y] = block.Color;
                                pixelDiffs[x, y] = -1;
                            }
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

            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    if (pixelDiffs[x, y] < 0)
                    {
                        var p1 = image[x, y];
                        var p2 = renderCanvas[x, y];
                        pixelDiffs[x, y] = PixelDiff(p1, p2);
                    }

                    diff += pixelDiffs[x, y];
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
