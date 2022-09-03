namespace Core
{
    public class Renderer
    {
        private readonly Canvas canvas;
        private readonly Image image;
        private readonly RGBA[,] renderCanvas;
        private readonly Dictionary<string, double> blockCost;
        private readonly double[,] pixelCost;

        public Renderer(Canvas canvas, Image image)
        {
            this.canvas = canvas;
            this.image = image;
            renderCanvas = new RGBA[canvas.Width, canvas.Height];
            blockCost = new Dictionary<string, double>();
            pixelCost = new double[canvas.Width, canvas.Height];

            // initialize
            for (int x = 0; x < canvas.Width; x++)
            {
                for (int y = 0; y < canvas.Height; y++)
                {
                    renderCanvas[x, y] = new RGBA(255, 255, 255, 255);
                    pixelCost[x, y] = -1;
                }
            }
        }

        public int GetImageCost()
        {
            double cost = 0;
            double alpha = 0.005;
            var blocks = canvas.Simplify();

            foreach (var block in blocks)
            {
                if (!block.HasRendered)
                {
                    RenderBlock(block);
                }

                cost += blockCost[block.ID];
            }

            return (int)Math.Round(cost * alpha);
        }

        private void RenderBlock(SimpleBlock block)
        {
            block.HasRendered = true;
            double cost = 0;

            for (var y = block.BottomLeft.Y; y < block.TopRight.Y; y++)
            {
                for (var x = block.BottomLeft.X; x < block.TopRight.X; x++)
                {
                    bool colorChanged = renderCanvas[x, y] != block.Color;
                    double pixelDiff = pixelCost[x, y];

                    if (colorChanged)
                    {
                        renderCanvas[x, y] = block.Color;
                        pixelDiff = -1;
                    }

                    if (pixelDiff < 0)
                    {
                        var p1 = image[x, y];
                        var p2 = renderCanvas[x, y];
                        pixelCost[x, y] = PixelDiff(p1, p2);
                    }

                    cost += pixelCost[x, y];
                }
            }

            blockCost[block.ID] = cost;
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
                                pixelCost[x, y] = -1;
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
                    if (pixelCost[x, y] < 0)
                    {
                        var p1 = image[x, y];
                        var p2 = renderCanvas[x, y];
                        pixelCost[x, y] = PixelDiff(p1, p2);
                    }

                    diff += pixelCost[x, y];
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
