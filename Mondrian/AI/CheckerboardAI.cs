using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace AI
{
    public class CheckerboardAI
    {
        public static readonly int LEVELS = 6;

        public static void Solve(Picasso picasso, AIArgs args, LoggerBase logger)
        {
            RecursiveSolveBad(picasso, picasso.AllBlocks.First(), 0, picasso.Score);
        }

        public static bool RecursiveSolveBad(Picasso picasso, Block block, int level, int scoreToBeat)
        {
            if (level == LEVELS)
            {
                return false;
            }

            Point center = new Point((block.TopRight.X - block.BottomLeft.X) / 2 + block.BottomLeft.X, (block.TopRight.Y - block.BottomLeft.Y) / 2 + block.BottomLeft.Y);
            var sample = SamplePoint(picasso, block, center);
            bool improved = false;
            if (picasso.Score < scoreToBeat)
            {
                scoreToBeat = picasso.Score;
                improved = true;
            }
            foreach (Block subBlock in sample.subBlocks)
            {
                improved |= RecursiveSolveBad(picasso, subBlock, level + 1, scoreToBeat);
            }

            if (!improved)
            {
                picasso.Undo(sample.instructionsUsed + 1);
            }

            return improved;
        }


        public static (int instructionsUsed, List<Block> subBlocks) SamplePoint(Picasso picasso, Block block, Point p)
        {
            List<Block> subBlocks = picasso.PointCut(block, p).ToList();

            // Color each the average of the canvas underneath
            int instructionsUsed = 0;
            foreach (Block subBlock in subBlocks)
            {
                int preScore = picasso.Score;
                picasso.Color(subBlock, picasso.AverageTargetColor(subBlock));
                int postScore = picasso.Score;
                if (postScore < preScore)
                {
                    instructionsUsed++;
                }
                else
                {
                    picasso.Undo(1);
                }
            }

            return (instructionsUsed, subBlocks);
        }
    }
}
