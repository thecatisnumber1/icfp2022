namespace Core
{
    public static class InstructionCostCalculator
    {
        private static Dictionary<InstructionType, int> costMap = new Dictionary<InstructionType, int>
        {
            { InstructionType.Nop, 0},
            { InstructionType.Comment, 0},
            { InstructionType.Color, 5},
            { InstructionType.PointCut, 10},
            { InstructionType.VerticalCut, 7},
            { InstructionType.HorizontalCut, 7},
            { InstructionType.Swap, 3},
            { InstructionType.Merge, 1},
        };

        public static int GetCost(InstructionType instructionType, int blockSize, int canvasSize)
        {
            int baseCost = costMap[instructionType];
            int totalCost = (int)Math.Round(baseCost * (canvasSize / (double)blockSize));
            return totalCost;
        }
    }
}
