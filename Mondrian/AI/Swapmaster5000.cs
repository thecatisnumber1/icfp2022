using Core;

namespace AI
{
    public class Swapmaster5000
    {
        public static void Solve(Picasso picasso, AIArgs args, LoggerBase logger)
        {
            var swaps = new Swapmaster6000(picasso, args, logger);
            swaps.Initialize();
            swaps.Run();
        }

        private class Swapmaster6000
        {
            private class VirtualBlock
            {
                public RGBA Color { get; set; }
                public string? ActualBlockId { get; set; }
                public IEnumerable<RGBA>? PreferredColors { get; set; }
            }

            private Picasso picasso;
            private LoggerBase logger;
            private VirtualBlock[,] virtualCanvas;
            private VirtualBlock[,] virtualImage;
            private int virtualSize;
            private int blockSize;

            public Swapmaster6000(Picasso picasso, AIArgs args, LoggerBase logger)
            {
                this.picasso = picasso;
                this.logger = logger;
                var blocks = picasso.AllSimpleBlocks;

                if (picasso.TargetImage.Height != picasso.TargetImage.Width)
                {
                    logger.LogMessage($"Non-square canvas? I can't handle that");
                }

                if (blocks.Count() < 2)
                {
                    logger.LogMessage($"Only one block? I can't handle that");
                }

                if (!IsBlockSquare(blocks.First()))
                {
                    logger.LogMessage($"Non-square blocks? I can't handle that");
                }

                if (!IsAllBlocksSameSize(blocks))
                {
                    logger.LogMessage($"All blocks not same size? I can't handle that");
                }

                blockSize = blocks.First().Size.X;
                virtualSize = picasso.TargetImage.Width / blockSize;

                virtualCanvas = new VirtualBlock[virtualSize, virtualSize];
                virtualImage = new VirtualBlock[virtualSize, virtualSize];
            }

            public void Initialize()
            {
                var blocks = picasso.AllSimpleBlocks;
                var colorSet = blocks.Select(x => x.Color).Distinct();

                GenerateVirtualCanvas(blocks);
                GenerateVirtualImage(picasso.TargetImage, colorSet);
            }

            public void Run()
            {
                for (int y = 0; y < virtualSize; y++)
                {
                    for (int x = 0; x < virtualSize; x++)
                    {
                        var vc = virtualCanvas[x, y];
                        var vi = virtualImage[x, y];

                        foreach (var targetColor in vi.PreferredColors)
                        {
                            if (vc.Color == targetColor)
                            {
                                break;
                            }

                            var (xx, yy) = FindSwapPartner(x, y, targetColor);

                            if (xx != -1)
                            {
                                PerformSwap(x, y, xx, yy);
                                logger.Render(picasso);
                                Thread.Sleep(100);
                                break;
                            }
                        }

                        
                    }
                }

                logger.Render(picasso);
            }

            private (int, int) FindSwapPartner(int startX, int startY, RGBA targetColor)
            {
                for (int y = startY; y < virtualSize; y++)
                {
                    for (int x = (y == startY ? startX + 1 : 0); x < virtualSize; x++)
                    {
                        var color = virtualCanvas[x, y].Color;

                        //logger.LogMessage($"Checking {x}, {y}");
                        
                        if (color == targetColor && virtualImage[x, y].PreferredColors.First() != color)
                        {
                            return (x, y);
                        }
                    }
                }

                return (-1, -1);
            }

            private void PerformSwap(int x1, int y1, int x2, int y2)
            {
                //logger.LogMessage($"Swap {x1},{y1} with {x2},{y2}");
                var block1 = virtualCanvas[x1, y1];
                var block2 = virtualCanvas[x2, y2];

                picasso.Swap(block1.ActualBlockId, block2.ActualBlockId);
                virtualCanvas[x1, y1] = block2;
                virtualCanvas[x2, y2] = block1;
            }


            private void GenerateVirtualImage(Image image, IEnumerable<RGBA> colorSet)
            {
                for (int x = 0; x < virtualSize; x++)
                {
                    for (int y = 0; y < virtualSize; y++)
                    {
                        var color = image.AverageColor(new Rectangle(
                            new Point(x*blockSize, y*blockSize),
                            new Point(x*blockSize+blockSize, y*blockSize+blockSize)));

                        virtualImage[x, y] = new VirtualBlock()
                        {
                            Color = color,
                            PreferredColors = colorSet.OrderBy(x => x.Diff(color)).ToArray()
                        };
                    }
                }
            }

            private void GenerateVirtualCanvas(IEnumerable<SimpleBlock> blocks)
            {
                foreach (var block in blocks)
                {
                    virtualCanvas[block.BottomLeft.X / blockSize, block.BottomLeft.Y / blockSize] = new VirtualBlock()
                    {
                        Color = block.Color,
                        ActualBlockId = block.ID
                    };
                }
            }

            private static bool IsBlockSquare(SimpleBlock block)
            {
                return block.Size.X == block.Size.Y;
            }

            private static bool IsAllBlocksSameSize(IEnumerable<SimpleBlock> blocks)
            {
                var sizeX = blocks.First().Size.X;
                var sizeY = blocks.First().Size.Y;
                return blocks.All(x => x.Size.X == sizeX && x.Size.Y == sizeY);
            }
        }
    }
}
