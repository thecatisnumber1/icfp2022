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

            if (args.problemNum != -1)
            {
                int score = picasso.Score;
                logger.LogMessage($"Score: {score}");
                
                Rest.CacheBests();

                int best = Rest.BestForProblem(args.problemNum);
                if (picasso.Score < best)
                {
                    logger.LogMessage($"Woo new top score! Previous: {best}.");
                    logger.LogMessage($"{Math.Round(score / (double)best * 100, 2)}% of previous high");
                }
                else
                {
                    logger.LogMessage($"Not good enough! Previous: {best}.");
                    logger.LogMessage($"{Math.Round(score / (double)best * 100, 2)}% of best solution.");
                }

                Rest.Upload(args.problemNum, string.Join("\n", picasso.SerializeInstructions()), picasso.Score);
            }
        }

        private class Swapmaster6000
        {
            private class VirtualBlock
            {
                public RGBA Color { get; set; }
                public string ActualBlockId { get; set; }
                public bool Locked { get; set; }
            }

            private Picasso picasso;
            private LoggerBase logger;
            private VirtualBlock[,] virtualCanvas;
            private int virtualSize;
            private int blockSize;

            private class Pri
            {
                public int x;
                public int y;
                public RGBA color;
                public double cost;
            }

            private List<Pri> priorities;

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
                priorities = new List<Pri>();
            }

            public void Initialize()
            {
                var blocks = picasso.AllSimpleBlocks;
                var colorSet = blocks.Select(x => x.Color).Distinct();

                GenerateVirtualCanvas(blocks);
                GeneratePriorities(picasso.TargetImage, colorSet);

                logger.Render(picasso);
            }

            public void Run()
            {
                for (int n = 0; n < priorities.Count; n++)
                {
                    var pri = priorities[n];
                    var vc = virtualCanvas[pri.x, pri.y];

                    if (!vc.Locked)
                    {
                        if (vc.Color == pri.color)
                        {
                            vc.Locked = true;
                        }
                        else
                        {
                            var (xx, yy) = FindSwapPartner(n, pri.color);
                            
                            if (xx != -1)
                            {
                                PerformSwap(pri.x, pri.y, xx, yy);
                            }
                        }
                    }
                }
            }

            private (int, int) FindSwapPartner(int upperBound, RGBA targetColor)
            {
                for (int i = priorities.Count - 1; i > upperBound; i--)
                {
                    var pri = priorities[i];
                    var swapOption = virtualCanvas[pri.x, pri.y];

                    if (!swapOption.Locked && pri.color == targetColor && swapOption.Color == targetColor)
                    {
                        return (pri.x, pri.y);
                    }
                }

                return (-1, -1);
            }
            
            private void PerformSwap(int x1, int y1, int x2, int y2)
            {
                //logger.LogMessage($"Swap {x1},{y1} with {x2},{y2}");
                var block1 = virtualCanvas[x1, y1];
                var block2 = virtualCanvas[x2, y2];

                int pre = picasso.Score;
                picasso.Swap(block1.ActualBlockId, block2.ActualBlockId);
                int post = picasso.Score;

                if (pre <= post)
                {
                    picasso.Undo();
                    return;
                }
                
                virtualCanvas[x1, y1] = block2;
                virtualCanvas[x2, y2] = block1;

                block2.Locked = true;

                logger.Render(picasso);
                Thread.Sleep(50);
            }

            private void GeneratePriorities(Image image, IEnumerable<RGBA> colorSet)
            {
                for (int x = 0; x < virtualSize; x++)
                {
                    for (int y = 0; y < virtualSize; y++)
                    {
                        var imageColor = image.AverageColor(new Rectangle(
                            new Point(x * blockSize, y * blockSize),
                            new Point(x * blockSize + blockSize, y * blockSize + blockSize)));

                        foreach (var color in colorSet)
                        {
                            priorities.Add(new Pri()
                            {
                                x = x,
                                y = y,
                                color = color,
                                cost = imageColor.Diff(color)
                            });
                        }
                    }
                }

                priorities = priorities.OrderBy(x => x.cost).ToList();
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
