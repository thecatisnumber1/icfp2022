using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace AI
{
    public class ScannerAI
    {
        public static List<Rectangle> Rects;
        public static void Solve(Picasso picasso, AIArgs args, LoggerBase logger)
        {
            Rects = new List<Rectangle>();
            Picasso temp = new Picasso(picasso.TargetImage);
            picasso.Color(picasso.AllBlocks.First().ID, picasso.AverageTargetColor(picasso.AllBlocks.First()));
            logger.Render(picasso);
            ScanBlock(picasso, picasso.AllBlocks.First(), logger);
            
            logger.LogMessage($"Scanner score = {picasso.Score}.");
        }

        public static void ScanBlock(Picasso picasso, Block block, LoggerBase logger)
        {
            int bestScore = picasso.Score;
            bool verticalBest = false;
            bool colorFirstBest = false;
            bool colorSecondBest = false;
            int index = -1;

            bool colorFirst;
            bool colorSecond;
            int movesToUndo;

            for (int x = block.BottomLeft.X + 1; x < block.TopRight.X - 1; x++)
            {
                colorFirst = false;
                colorSecond = false;
                movesToUndo = 1;

                List<Block> blocks = picasso.VerticalCut(block.ID, x).ToList();
                if (ColorAndTest(picasso, blocks[0]))
                {
                    colorFirst = true;
                    ++movesToUndo;
                }
                if (ColorAndTest(picasso, blocks[1]))
                {
                    colorSecond = true;
                    ++movesToUndo;
                }

                if (picasso.Score < bestScore)
                {
                    verticalBest = true;
                    colorFirstBest = colorFirst;
                    colorSecondBest = colorSecond;
                    bestScore = picasso.Score;
                    index = x;
                }

                picasso.Undo(movesToUndo);
            }

            for (int y = block.BottomLeft.Y + 1; y < block.TopRight.Y - 1; y++)
            {
                colorFirst = false;
                colorSecond = false;
                movesToUndo = 1;

                List<Block> blocks = picasso.HorizontalCut(block.ID, y).ToList();
                if (ColorAndTest(picasso, blocks[0]))
                {
                    colorFirst = true;
                    ++movesToUndo;
                }
                if (ColorAndTest(picasso, blocks[1]))
                {
                    colorSecond = true;
                    ++movesToUndo;
                }

                if (picasso.Score < bestScore)
                {
                    verticalBest = false;
                    colorFirstBest = colorFirst;
                    colorSecondBest = colorSecond;
                    bestScore = picasso.Score;
                    index = y;
                }

                picasso.Undo(movesToUndo);
            }

            if (bestScore >= picasso.Score)
            {
                return;
            }

            List<Block> nextBlocks;
            if (verticalBest) nextBlocks = picasso.VerticalCut(block.ID, index).ToList();
            else nextBlocks = picasso.HorizontalCut(block.ID, index).ToList();

            if (colorFirstBest) picasso.Color(nextBlocks[0].ID, picasso.AverageTargetColor(nextBlocks[0]));
            if (colorSecondBest) picasso.Color(nextBlocks[1].ID, picasso.AverageTargetColor(nextBlocks[1]));
            logger.Render(picasso);

            ScanBlock(picasso, nextBlocks[0], logger);
            ScanBlock(picasso, nextBlocks[1], logger);
        }

        private static bool ColorAndTest(Picasso picasso, Block block)
        {
            int tempScore = picasso.Score;
            picasso.Color(block.ID, picasso.AverageTargetColor(block));
            if (picasso.Score < tempScore)
            {
                return true;
            }

            picasso.Undo(1);
            return false;
        }
    }
}
