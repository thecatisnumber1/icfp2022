using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI
{
    public static class LinePrinter
    {
        private static LoggerBase logger;
        public static void SolveH(Core.Picasso picasso, AIArgs args, LoggerBase loggerr)
        {
            logger = loggerr;
            SolveHoriz(picasso, picasso.AllBlocks.First());
            logger.LogMessage($"Score WOO {picasso.Score}");
        }

        public static void SolveV(Core.Picasso picasso, AIArgs args, LoggerBase loggerr)
        {
            logger = loggerr;
            SolveVert(picasso, picasso.AllBlocks.First());
            logger.LogMessage($"Score WOO {picasso.Score}");
        }

        public static void SolveD(Core.Picasso picasso, AIArgs args, LoggerBase loggerr)
        {
            logger = loggerr;

            DotMatrix(picasso, picasso.AllBlocks.First());
            logger.LogMessage($"Score WOO {picasso.Score}");
        }

        public static RGBA GetBlockColor(Block block)
        {
            var sblock = block as SimpleBlock;
            if (sblock != null)
                return sblock.Color;
            else
            {
                var cblock = block as ComplexBlock;
                return cblock.GetChildren().First().Color;
            }

        }

        public static void DotMatrix(Core.Picasso picasso, Block block)
        {
            var bg = picasso.TargetImage[block.BottomLeft];
            picasso.Color(block.ID, bg);
            for (int y = block.BottomLeft.Y; y < block.TopRight.Y - 1; y++)
            {
                var leftColor = picasso.TargetImage[block.BottomLeft.X, y];
                if (GetBlockColor(block) != leftColor)
                    picasso.Color(block.ID, leftColor);
                
                var blocks = picasso.HorizontalCut(block.ID, y + 1);
                var line = blocks.First();
                var rest = blocks.Last();

                var curSplit = line;
                bool work = false;
                double error = 0;
                for (int x = line.BottomLeft.X; x < line.TopRight.X; x++)
                {
                    var curColor = picasso.TargetImage[x, y];
                    error += leftColor.Diff(curColor);
                    if (error < 200) continue;

                    blocks = picasso.VerticalCut(curSplit.ID, x - 1);
                    curSplit = blocks.Last();
                    picasso.Color(curSplit.ID, curColor);
                    leftColor = curColor;
                    work = true;
                    error = 0;
                }

                if (!work)
                {
                    picasso.Undo();
                    continue;
                }

                block = rest;
                logger.Render(picasso);
            }
            logger.Render(picasso);


        }

        public static void SolveHoriz(Core.Picasso picasso, Block block)
        {
            for (int i = block.BottomLeft.Y; i < block.TopRight.Y - 1; i++)
            {
                var blocks = picasso.HorizontalCut(block.ID, i + 1);
                var block0 = blocks.First() as SimpleBlock;
                var block1 = blocks.Last() as SimpleBlock;
                var color = picasso.AverageTargetColor(block0);
                picasso.Undo();

                if (color == block0.Color) continue;
                picasso.Color(block.ID, color);
                blocks = picasso.HorizontalCut(block.ID, i + 1);
                block = block1;
                logger.Render(picasso);
            }
        }

        public static void SolveVert(Core.Picasso picasso, Block block)
        {
            for (int i = block.BottomLeft.X; i < block.TopRight.X - 1; i++)
            {
                var blocks = picasso.VerticalCut(block.ID, i + 1);
                var block0 = blocks.First() as SimpleBlock;
                var block1 = blocks.Last() as SimpleBlock;
                var color = picasso.AverageTargetColor(block0);
                picasso.Undo();

                if (color == block0.Color) continue;
                picasso.Color(block.ID, color);
                blocks = picasso.VerticalCut(block.ID, i + 1);
                block = block1;
                logger.Render(picasso);
            }
        }

    }
}
