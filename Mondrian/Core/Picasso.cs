using static Core.InstructionCostCalculator;
using static Core.Renderer;

namespace Core
{
    public class Picasso
    {
        private struct Snack
        {
            public Instruction Instruction;
            public int Cost;
            public Block Backup;

            public Snack(Instruction instruction, int cost, Block backup)
            {
                Instruction = instruction;
                Cost = cost;
                Backup = backup;
            }
        }

        private readonly Canvas canvas;
        private readonly int canvasSize;

        private readonly Stack<Snack> instructions;
        private int totalInstructionCost;

        private readonly Image image;

        public Picasso(Image img)
        {
            image = img;
            canvas = new Canvas(image.Width, image.Height, new RGBA());
            canvasSize = canvas.Size.GetScalarSize();
            instructions = new Stack<Snack>();
        }

        public int Score
        {
            get
            {
                return totalInstructionCost + GetImageCost(canvas, image);
            }
        }

        public IEnumerable<Block> AllBlocks
        {
            get
            {
                return canvas.Blocks.Values;
            }
        }

        public void Color(Block block, RGBA color)
        {
            int cost = GetCost(InstructionType.Color, block.Size.GetScalarSize(), canvasSize);
            var snack = new Snack(new ColorInstruction(block.ID, color), cost, canvas.Blocks[block.ID]);

            instructions.Push(snack);
            totalInstructionCost += cost;

            canvas.Blocks[block.ID] = new SimpleBlock(
                block.ID,
                block.BottomLeft.Clone(),
                block.TopRight.Clone(),
                color);
        }

        public IEnumerable<Block> PointCut(Block block, Point point)
        {
            // TBD update blocks

            return new Block[0];
        }

        public RGBA AverageTargetColor(Block block)
        {
            // TBD
            return new RGBA();
        }

        public void Undo()
        {
            var snack = instructions.Pop();
            totalInstructionCost -= snack.Cost;
            canvas.Blocks[snack.Backup.ID] = snack.Backup;
        }

        public void Undo(int count)
        {
            for (int i = 0; i < count && instructions.Count > 0; i++)
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
