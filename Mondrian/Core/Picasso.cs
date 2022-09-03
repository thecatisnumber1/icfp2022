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
        }

        private readonly Canvas canvas;
        private readonly int canvasSize;

        private readonly Stack<Snack> instructions;
        private int totalInstructionCost;

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
                return totalInstructionCost + renderer.GetImageCost();
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
            totalInstructionCost += cost;

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
            totalInstructionCost += cost;

            var newBlocks = new List<Block>();

            if (oldBlock is SimpleBlock sBlock) {
                newBlocks.Add(new SimpleBlock(
                    sBlock.ID + ".0",
                    sBlock.BottomLeft.Clone(),
                    point.Clone(),
                    sBlock.Color
                ));
                newBlocks.Add(new SimpleBlock(
                    sBlock.ID + ".1",
                    new Point(point.X, sBlock.BottomLeft.Y),
                    new Point(sBlock.TopRight.X, point.Y),
                    sBlock.Color
                ));
                newBlocks.Add(new SimpleBlock(
                    sBlock.ID + ".2",
                    point.Clone(),
                    sBlock.TopRight.Clone(),
                    sBlock.Color
                ));
                newBlocks.Add(new SimpleBlock(
                    sBlock.ID + ".3",
                    new Point(sBlock.BottomLeft.X, point.Y),
                    new Point(point.X, sBlock.TopRight.Y),
                    sBlock.Color
                ));
                canvas.Blocks.Remove(sBlock.ID);
                newBlocks.ForEach(block => canvas.Blocks[block.ID] = block);
            }

            //... TBD complexblocks

            instructions.Push(new Snack(new PointCutInstruction(blockId, point), cost, oldBlock, newBlocks.Select(block => block.ID)));

            return newBlocks;
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
                totalInstructionCost -= snack.Cost;

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
