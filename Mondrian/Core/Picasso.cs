using static Core.InstructionCostCalculator;

namespace Core
{
    public class Picasso
    {
        private struct Snack
        {
            public Instruction Instruction;
            public int Cost;
            public IEnumerable<Block> RemovedBlocks;
            public IEnumerable<string> AddedIds;

            public Snack(Instruction instruction, int cost, IEnumerable<Block> removed, IEnumerable<string> added)
            {
                Instruction = instruction;
                Cost = cost;
                RemovedBlocks = removed;
                AddedIds = added;
            }

            public Snack(Instruction instruction, int cost, Block removed, string added)
                : this(instruction, cost, new List<Block>() { removed }, new List<string>() { added })
            { }

            public Snack(Instruction instruction, int cost, Block removed, IEnumerable<string> added)
                : this(instruction, cost, new List<Block>() { removed }, added)
            { }

            public Snack(Instruction instruction, int cost, IEnumerable<Block> removed, string added)
                : this(instruction, cost, removed, new List<string>() { added })
            { }
        }

        private int topLevelIdCounter = 0;

        private readonly Canvas canvas;
        private readonly int canvasSize;

        private readonly Stack<Snack> instructions;
        public int TotalInstructionCost
        {
            get; private set;
        }

        public readonly Image TargetImage;
        private readonly Renderer renderer;

        public Picasso(Image img)
        {
            TargetImage = img;
            canvas = new Canvas(TargetImage.Width, TargetImage.Height, new RGBA(255, 255, 255, 255)); // This is FFFFFFFF per spec.
            canvasSize = canvas.Size;
            renderer = new Renderer(canvas, TargetImage);
            instructions = new Stack<Snack>();
        }

        public int Score
        {
            get
            {
                return TotalInstructionCost + renderer.GetImageCost();
            }
        }

        public int InstructionCount
        {
            get
            {
                return instructions.Count;
            }
        }

        public IEnumerable<Block> AllBlocks
        {
            get
            {
                return canvas.Blocks.Values;
            }
        }

        public IEnumerable<SimpleBlock> AllSimpleBlocks => canvas.Simplify();

        public void Color(string blockId, RGBA color)
        {
            if (!canvas.Blocks.ContainsKey(blockId))
            {
                throw new Exception($"Unknown blockId [{blockId}]");
            }

            var oldBlock = canvas.Blocks[blockId];

            int cost = GetCost(InstructionType.Color, oldBlock.Size.GetScalarSize(), canvasSize);
            TotalInstructionCost += cost;

            canvas.Blocks[blockId] = new SimpleBlock(
                blockId,
                oldBlock.BottomLeft.Clone(),
                oldBlock.TopRight.Clone(),
                color);

            instructions.Push(new Snack(new ColorInstruction(blockId, color), cost, oldBlock, blockId));
        }

        public IEnumerable<Block> PointCut(string blockId, Point point)
        {
            if (!canvas.Blocks.ContainsKey(blockId))
            {
                throw new Exception($"Unknown blockId [{blockId}]");
            }

            var oldBlock = canvas.Blocks[blockId];

            if (!point.IsStrictlyInside(oldBlock.BottomLeft, oldBlock.TopRight))
            {
                throw new Exception($"Point is outside [{blockId}]! Block is from {oldBlock.BottomLeft} to {oldBlock.TopRight}, point is at {point}!");
            }

            int cost = GetCost(InstructionType.PointCut, oldBlock.Size.GetScalarSize(), canvasSize);
            TotalInstructionCost += cost;

            var newBlocks = new List<Block>();

            if (oldBlock is SimpleBlock sBlock) {
                newBlocks.Add(new SimpleBlock(
                    sBlock.ID + ".3",
                    sBlock.BottomLeft.Clone(),
                    point.Clone(),
                    sBlock.Color
                ));
                newBlocks.Add(new SimpleBlock(
                    sBlock.ID + ".2",
                    new Point(point.X, sBlock.BottomLeft.Y),
                    new Point(sBlock.TopRight.X, point.Y),
                    sBlock.Color
                ));
                newBlocks.Add(new SimpleBlock(
                    sBlock.ID + ".1",
                    point.Clone(),
                    sBlock.TopRight.Clone(),
                    sBlock.Color
                ));
                newBlocks.Add(new SimpleBlock(
                    sBlock.ID + ".0",
                    new Point(sBlock.BottomLeft.X, point.Y),
                    new Point(point.X, sBlock.TopRight.Y),
                    sBlock.Color
                ));
            }

            if (oldBlock is ComplexBlock cBlock)
            {
                var bottomLeftBlocks = new List<SimpleBlock>();
                var bottomRightBlocks = new List<SimpleBlock>();
                var topLeftBlocks = new List<SimpleBlock>();
                var topRightBlocks = new List<SimpleBlock>();

                foreach (var block in cBlock.SubBlocks)
                {
                    if (block.BottomLeft.X >= point.X && block.BottomLeft.Y >= point.Y)
                    {
                        topRightBlocks.Add(block);
                    }
                    else if (block.TopRight.X <= point.X && block.TopRight.Y <= point.Y)
                    {
                        bottomLeftBlocks.Add(block);
                    }
                    else if (block.TopRight.X <= point.X && block.BottomLeft.Y >= point.Y)
                    {
                        topLeftBlocks.Add(block);
                    }
                    else if (block.BottomLeft.X >= point.X && block.TopRight.Y <= point.Y)
                    {
                        bottomRightBlocks.Add(block);
                    }
                    else if (point.IsInside(block.BottomLeft, block.TopRight))
                    {
                        bottomLeftBlocks.Add(new SimpleBlock(
                            "bl_child",
                            block.BottomLeft.Clone(),
                            point,
                            block.Color
                        ));
                        bottomRightBlocks.Add(new SimpleBlock(
                            "br_child",
                            new Point(point.X, block.BottomLeft.Y),
                            new Point(block.TopRight.X, point.Y),
                            block.Color
                        ));
                        topRightBlocks.Add(new SimpleBlock(
                            "tr_child",
                            point,
                            block.TopRight.Clone(),
                            block.Color
                        ));
                        topLeftBlocks.Add(new SimpleBlock(
                            "tl_child",
                            new Point(block.BottomLeft.X, point.Y),
                            new Point(point.X, block.TopRight.Y),
                            block.Color
                        ));
                    }
                    else if (block.BottomLeft.X <= point.X && 
                        point.X <= block.TopRight.X &&
                        point.Y < block.BottomLeft.Y)
                    {
                        topLeftBlocks.Add(new SimpleBlock(
                            "case2_tl_child",
                            block.BottomLeft.Clone(),
                            new Point(point.X, block.TopRight.Y),
                            block.Color
                        ));
                        topRightBlocks.Add(new SimpleBlock(
                            "case2_tr_child",
                            new Point(point.X, block.BottomLeft.Y),
                            block.TopRight.Clone(),
                            block.Color
                        ));
                    }
                    else if (block.BottomLeft.X <= point.X &&
                        point.X <= block.TopRight.X &&
                        point.Y > block.TopRight.Y)
                    {
                        bottomLeftBlocks.Add(new SimpleBlock(
                            "case8_bl_child",
                            block.BottomLeft.Clone(),
                            new Point(point.X, block.TopRight.Y),
                            block.Color
                        ));
                        bottomRightBlocks.Add(new SimpleBlock(
                            "case8_br_child",
                            new Point(point.X, block.BottomLeft.Y),
                            block.TopRight.Clone(),
                            block.Color
                        ));
                    }
                    else if (block.BottomLeft.Y <= point.Y &&
                        point.Y <= block.TopRight.Y &&
                        point.X < block.BottomLeft.X)
                    {
                        bottomRightBlocks.Add(new SimpleBlock(
                            "case4_br_child",
                            block.BottomLeft.Clone(),
                            new Point(block.TopRight.X, point.Y),
                            block.Color
                        ));
                        topRightBlocks.Add(new SimpleBlock(
                            "case4_tr_child",
                            new Point(block.BottomLeft.X, point.Y),
                            block.TopRight.Clone(),
                            block.Color
                        ));
                    }
                    else if (block.BottomLeft.Y <= point.Y &&
                        point.Y <= block.TopRight.Y &&
                        point.X > block.TopRight.X)
                    {
                        bottomLeftBlocks.Add(new SimpleBlock(
                            "case6_bl_child",
                            block.BottomLeft.Clone(),
                            new Point(block.TopRight.X, point.Y),
                            block.Color
                        ));
                        topLeftBlocks.Add(new SimpleBlock(
                            "case6_br_child",
                            new Point(block.BottomLeft.X, point.Y),
                            block.TopRight.Clone(),
                            block.Color
                        ));
                    }
                }

                newBlocks.Add(new ComplexBlock(
                    cBlock.ID + ".3",
                    cBlock.BottomLeft.Clone(),
                    point.Clone(),
                    bottomLeftBlocks.Where((b) => b.Size.GetScalarSize() > 0).ToArray()
                ));
                newBlocks.Add(new ComplexBlock(
                    cBlock.ID + ".2",
                    new Point(point.X, cBlock.BottomLeft.Y),
                    new Point(cBlock.TopRight.X, point.Y),
                    bottomRightBlocks.Where((b) => b.Size.GetScalarSize() > 0).ToArray()
                ));
                newBlocks.Add(new ComplexBlock(
                    cBlock.ID + ".1",
                    point.Clone(),
                    cBlock.TopRight.Clone(),
                    topRightBlocks.Where((b) => b.Size.GetScalarSize() > 0).ToArray()
                ));
                newBlocks.Add(new ComplexBlock(
                    cBlock.ID + ".0",
                    new Point(cBlock.BottomLeft.X, point.Y),
                    new Point(point.X, cBlock.TopRight.Y),
                    topLeftBlocks.Where((b) => b.Size.GetScalarSize() > 0).ToArray()
                ));

            }

            canvas.Blocks.Remove(blockId);
            newBlocks.ForEach(block => canvas.Blocks[block.ID] = block);

            instructions.Push(new Snack(new PointCutInstruction(blockId, point), cost, oldBlock, newBlocks.Select(block => block.ID)));

            return newBlocks;
        }

        public IEnumerable<Block> VerticalCut(string blockId, int lineNumber)
        {
            if (!canvas.Blocks.ContainsKey(blockId))
            {
                throw new Exception($"Unknown blockId [{blockId}]");
            }

            var oldBlock = canvas.Blocks[blockId];

            if (!(oldBlock.BottomLeft.X <= lineNumber && lineNumber <= oldBlock.TopRight.X))
            {
                throw new Exception($"Line number is outside [{blockId}]! Block is from {oldBlock.BottomLeft} to {oldBlock.TopRight}, line is at {lineNumber}!");
            }

            int cost = GetCost(InstructionType.VerticalCut, oldBlock.Size.GetScalarSize(), canvasSize);
            TotalInstructionCost += cost;

            var newBlocks = new List<Block>();

            if (oldBlock is SimpleBlock sBlock)
            {
                newBlocks.Add(new SimpleBlock(
                    sBlock.ID + ".0",
                    sBlock.BottomLeft.Clone(),
                    new Point(lineNumber, sBlock.TopRight.Y),
                    sBlock.Color
                ));
                newBlocks.Add(new SimpleBlock(
                    sBlock.ID + ".1",
                    new Point(lineNumber, sBlock.BottomLeft.Y),
                    sBlock.TopRight.Clone(),
                    sBlock.Color
                ));
            }

            if (oldBlock is ComplexBlock cBlock)
            {
                var leftBlocks = new List<SimpleBlock>();
                var rightBlocks = new List<SimpleBlock>();

                foreach (var block in cBlock.SubBlocks)
                {
                    if (block.BottomLeft.X >= lineNumber)
                    {
                        rightBlocks.Add(block);
                    }
                    else if (block.TopRight.X <= lineNumber)
                    {
                        leftBlocks.Add(block);
                    }
                    else
                    {
                        leftBlocks.Add(new SimpleBlock(
                            "child",
                            block.BottomLeft.Clone(),
                            new Point(lineNumber, block.TopRight.Y),
                            block.Color
                        ));
                        rightBlocks.Add(new SimpleBlock(
                            "child",
                            new Point(lineNumber, block.BottomLeft.Y),
                            block.TopRight.Clone(),
                            block.Color
                        ));
                    }
                }

                newBlocks.Add(new ComplexBlock(
                    cBlock.ID + ".0",
                    cBlock.BottomLeft.Clone(),
                    new Point(lineNumber, cBlock.TopRight.Y),
                    leftBlocks.ToArray()
                ));
                newBlocks.Add(new ComplexBlock(
                    cBlock.ID + ".1",
                    new Point(lineNumber, cBlock.BottomLeft.Y),
                    cBlock.TopRight.Clone(),
                    rightBlocks.ToArray()
                ));
            }

            canvas.Blocks.Remove(blockId);
            newBlocks.ForEach(block => canvas.Blocks[block.ID] = block);

            instructions.Push(new Snack(new VerticalCutInstruction(blockId, lineNumber), cost, oldBlock, newBlocks.Select(block => block.ID)));

            return newBlocks;
        }

        public IEnumerable<Block> HorizontalCut(string blockId, int lineNumber)
        {
            if (!canvas.Blocks.ContainsKey(blockId))
            {
                throw new Exception($"Unknown blockId [{blockId}]");
            }

            var oldBlock = canvas.Blocks[blockId];

            if (!(oldBlock.BottomLeft.Y <= lineNumber && lineNumber <= oldBlock.TopRight.Y))
            {
                throw new Exception($"Line number is outside [{blockId}]! Block is from {oldBlock.BottomLeft} to {oldBlock.TopRight}, line is at {lineNumber}!");
            }

            int cost = GetCost(InstructionType.HorizontalCut, oldBlock.Size.GetScalarSize(), canvasSize);
            TotalInstructionCost += cost;

            var newBlocks = new List<Block>();

            if (oldBlock is SimpleBlock sBlock)
            {
                newBlocks.Add(new SimpleBlock(
                    sBlock.ID + ".0",
                    sBlock.BottomLeft.Clone(),
                    new Point(sBlock.TopRight.X, lineNumber),
                    sBlock.Color
                ));
                newBlocks.Add(new SimpleBlock(
                    sBlock.ID + ".1",
                    new Point(sBlock.BottomLeft.X, lineNumber),
                    sBlock.TopRight.Clone(),
                    sBlock.Color
                ));
            }

            if (oldBlock is ComplexBlock cBlock)
            {
                var bottomBlocks = new List<SimpleBlock>();
                var topBlocks = new List<SimpleBlock>();

                foreach (var block in cBlock.SubBlocks)
                {
                    if (block.BottomLeft.Y >= lineNumber)
                    {
                        topBlocks.Add(block);
                    }
                    else if (block.TopRight.Y <= lineNumber)
                    {
                        bottomBlocks.Add(block);
                    }
                    else
                    {
                        bottomBlocks.Add(new SimpleBlock(
                            "child",
                            block.BottomLeft.Clone(),
                            new Point(block.TopRight.X, lineNumber),
                            block.Color
                        ));
                        topBlocks.Add(new SimpleBlock(
                            "child",
                            new Point(block.BottomLeft.X, lineNumber),
                            block.TopRight.Clone(),
                            block.Color
                        ));
                    }
                }

                newBlocks.Add(new ComplexBlock(
                    cBlock.ID + ".0",
                    cBlock.BottomLeft.Clone(),
                    new Point(cBlock.TopRight.X, lineNumber),
                    bottomBlocks.ToArray()
                ));
                newBlocks.Add(new ComplexBlock(
                    cBlock.ID + ".1",
                    new Point(cBlock.BottomLeft.X, lineNumber),
                    cBlock.TopRight.Clone(),
                    topBlocks.ToArray()
                ));
            }

            canvas.Blocks.Remove(blockId);
            newBlocks.ForEach(block => canvas.Blocks[block.ID] = block);

            instructions.Push(new Snack(new HorizontalCutInstruction(blockId, lineNumber), cost, oldBlock, newBlocks.Select(block => block.ID)));

            return newBlocks;
        }

        public ComplexBlock Merge(string blockId1, string blockId2)
        {
            if (!canvas.Blocks.ContainsKey(blockId1))
            {
                throw new Exception($"Unknown blockId1 [{blockId1}]");
            }

            if (!canvas.Blocks.ContainsKey(blockId2))
            {
                throw new Exception($"Unknown blockId2 [{blockId2}]");
            }

            var oldBlock1 = canvas.Blocks[blockId1];
            var oldBlock2 = canvas.Blocks[blockId2];

            int cost = GetCost(InstructionType.Merge, Math.Max(oldBlock1.Size.GetScalarSize(), oldBlock2.Size.GetScalarSize()), canvasSize);
            TotalInstructionCost += cost;

            bool bottomToTop = (oldBlock1.BottomLeft.Y == oldBlock2.TopRight.Y || oldBlock1.TopRight.Y == oldBlock2.BottomLeft.Y) &&
                    oldBlock1.BottomLeft.X == oldBlock2.BottomLeft.X &&
                    oldBlock1.TopRight.X == oldBlock2.TopRight.X;

            bool leftToRight = (oldBlock1.BottomLeft.X == oldBlock2.TopRight.X || oldBlock1.TopRight.X == oldBlock2.BottomLeft.X) &&
                    oldBlock1.BottomLeft.Y == oldBlock2.BottomLeft.Y &&
                    oldBlock1.TopRight.Y == oldBlock2.TopRight.Y;

            if (!bottomToTop && !leftToRight)
            {
                throw new Exception($"Blocks [{blockId1}] and [{blockId2}] are not mergable.");
            }

            topLevelIdCounter++;
            Point newBottomLeft, newTopRight;

            if (bottomToTop)
            {
                if (oldBlock1.BottomLeft.Y < oldBlock2.BottomLeft.Y)
                {
                    newBottomLeft = oldBlock1.BottomLeft;
                    newTopRight = oldBlock2.TopRight;
                }
                else
                {
                    newBottomLeft = oldBlock2.BottomLeft;
                    newTopRight = oldBlock1.TopRight;
                }
            }
            else
            {
                if (oldBlock1.BottomLeft.X < oldBlock2.BottomLeft.X)
                {
                    newBottomLeft = oldBlock1.BottomLeft;
                    newTopRight = oldBlock2.TopRight;
                }
                else
                {
                    newBottomLeft = oldBlock2.BottomLeft;
                    newTopRight = oldBlock1.TopRight;
                }
            }

            var newBlock = new ComplexBlock(
                    $"{topLevelIdCounter}",
                    newBottomLeft.Clone(),
                    newTopRight.Clone(),
                    oldBlock1.GetChildren().Concat(oldBlock2.GetChildren()).Select(block => block.Clone()).ToArray()
                );

            canvas.Blocks.Remove(blockId1);
            canvas.Blocks.Remove(blockId2);
            canvas.Blocks[newBlock.ID] = newBlock;

            instructions.Push(new Snack(new MergeInstruction(blockId1, blockId2), cost, new List<Block>() { oldBlock1, oldBlock2 }, newBlock.ID));

            return newBlock;
        }

        public IEnumerable<Block> Swap(string blockId1, string blockId2)
        {
            if (!canvas.Blocks.ContainsKey(blockId1))
            {
                throw new Exception($"Unknown blockId1 [{blockId1}]");
            }

            if (!canvas.Blocks.ContainsKey(blockId2))
            {
                throw new Exception($"Unknown blockId2 [{blockId2}]");
            }

            var oldBlock1 = canvas.Blocks[blockId1];
            var oldBlock2 = canvas.Blocks[blockId2];

            int cost = GetCost(InstructionType.Swap, oldBlock1.Size.GetScalarSize(), canvasSize);
            TotalInstructionCost += cost;

            if (oldBlock1.Size.X != oldBlock2.Size.X || oldBlock1.Size.Y != oldBlock2.Size.Y)
            {
                throw new Exception($"Blocks are not the same size, [${blockId1}] has size[${oldBlock1.Size.X},${oldBlock1.Size.Y}] while [${blockId2}] has size[${oldBlock2.Size.X},${oldBlock2.Size.Y}]");
            }

            Block newBlock1, newBlock2;

            if (oldBlock1 is SimpleBlock sBlock1)
            {
                newBlock2 = new SimpleBlock(
                    blockId1,
                    oldBlock2.BottomLeft.Clone(),
                    oldBlock2.TopRight.Clone(),
                    sBlock1.Color
                );
            }
            else// if (oldBlock1 is ComplexBlock cBlock1)
            {
                newBlock2 = new ComplexBlock(
                    blockId1,
                    oldBlock2.BottomLeft.Clone(),
                    oldBlock2.TopRight.Clone(),
                    ((ComplexBlock)oldBlock1).OffsetChildren(oldBlock2.BottomLeft)
                );
            }

            if (oldBlock2 is SimpleBlock sBlock2)
            {
                newBlock1 = new SimpleBlock(
                    blockId2,
                    oldBlock1.BottomLeft.Clone(),
                    oldBlock1.TopRight.Clone(),
                    sBlock2.Color
                );
            }
            else// if (oldBlock2 is ComplexBlock cBlock2)
            {
                newBlock1 = new ComplexBlock(
                    blockId2,
                    oldBlock1.BottomLeft.Clone(),
                    oldBlock1.TopRight.Clone(),
                    ((ComplexBlock)oldBlock2).OffsetChildren(oldBlock1.BottomLeft)
                );
            }

            canvas.Blocks[blockId1] = newBlock1;
            canvas.Blocks[blockId2] = newBlock2;

            instructions.Push(new Snack(new SwapInstruction(blockId1, blockId2), cost, new List<Block>() { oldBlock1, oldBlock2 }, new List<string>() { newBlock1.ID, newBlock2.ID }));

            return new List<Block>() { newBlock1, newBlock2 };
        }

        public RGBA AverageTargetColor(Block block)
        {
            return TargetImage.AverageColor(new Rectangle(block.BottomLeft, block.TopRight));
        }

        public void Undo()
        {
            if (instructions.Count > 0)
            {
                var snack = instructions.Pop();
                TotalInstructionCost -= snack.Cost;

                foreach (string id in snack.AddedIds)
                {
                    canvas.Blocks.Remove(id);
                }

                foreach (var block in snack.RemovedBlocks)
                {
                    block.HasRendered = false;
                    canvas.Blocks[block.ID] = block;
                }
            }
        }

        public void Undo(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Undo();
            }
        }

        public List<string> SerializeInstructions()
        {
            return instructions.Select(x => x.Instruction.ToString()).Reverse().ToList();
        }
    }
}
