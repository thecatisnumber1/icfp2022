using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace AI
{
    public class ScannerAI
    {
        public static void Solve(Picasso picasso, AIArgs args, LoggerBase logger)
        {
            picasso.Color(picasso.AllBlocks.First().ID, picasso.AverageTargetColor(picasso.AllBlocks.First()));
            logger.Render(picasso);
        }

        public static void ScanBlock(Picasso picasso, Block block)
        {
            int bestScore = picasso.Score;
            bool verticalBest = false;
            int index = -1;

            for (int x = block.BottomLeft.X + 1; x < block.TopRight.X; x++)
            {
            }
        }
    }
}
