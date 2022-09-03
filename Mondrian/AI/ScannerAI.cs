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
            ScanBlock(picasso, picasso.AllBlocks.First(), logger);
            //int scannerScore = picasso.Score;
            //picasso.Undo(picasso.InstructionCount);
            //CheckerboardAI.Solve(picasso, args, new ConsoleLogger());
            //logger.LogMessage($"Scanner score = {scannerScore}, Checkerboard score = {picasso.Score}.");
            //logger.Render(picasso);
        }

        public static void ScanBlock(Picasso picasso, Block block, LoggerBase logger)
        {
            int bestScore = picasso.Score;
            bool verticalBest = false;
            int index = -1;

            for (int x = block.BottomLeft.X + 1; x < block.TopRight.X - 1; x++)
            {
                List<Block> blocks = picasso.VerticalCut(block.ID, x).ToList();
                picasso.Color(blocks[0].ID, picasso.AverageTargetColor(blocks[0]));
                picasso.Color(blocks[1].ID, picasso.AverageTargetColor(blocks[1]));
                if (picasso.Score < bestScore)
                {
                    verticalBest = true;
                    bestScore = picasso.Score;
                    index = x;
                }

                picasso.Undo(3);
            }

            for (int y = block.BottomLeft.Y + 1; y < block.TopRight.Y - 1; y++)
            {
                List<Block> blocks = picasso.HorizontalCut(block.ID, y).ToList();
                picasso.Color(blocks[0].ID, picasso.AverageTargetColor(blocks[0]));
                picasso.Color(blocks[1].ID, picasso.AverageTargetColor(blocks[1]));
                if (picasso.Score < bestScore)
                {
                    verticalBest = false;
                    bestScore = picasso.Score;
                    index = y;
                }

                picasso.Undo(3);
            }

            if (bestScore >= picasso.Score)
            {
                return;
            }

            List<Block> nextBlocks;
            if (verticalBest)
            {
                nextBlocks = picasso.VerticalCut(block.ID, index).ToList();
                picasso.Color(nextBlocks[0].ID, picasso.AverageTargetColor(nextBlocks[0]));
                picasso.Color(nextBlocks[1].ID, picasso.AverageTargetColor(nextBlocks[1]));
                logger.Render(picasso);
            }
            else
            {
                nextBlocks = picasso.HorizontalCut(block.ID, index).ToList();
                picasso.Color(nextBlocks[0].ID, picasso.AverageTargetColor(nextBlocks[0]));
                picasso.Color(nextBlocks[1].ID, picasso.AverageTargetColor(nextBlocks[1]));
                logger.Render(picasso);

            }

            ScanBlock(picasso, nextBlocks[0], logger);
            ScanBlock(picasso, nextBlocks[1], logger);
        }
    }
}
