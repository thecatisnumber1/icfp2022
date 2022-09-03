﻿using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI
{
    public static class AllCutsAI
    {
        private static LoggerBase Logger;
        private static Point MinRes = new Point(20, 20);

        public static void Solve(Core.Picasso picasso, AIArgs args, LoggerBase logger)
        {
            Logger = logger;
            Recurse(picasso, picasso.AllBlocks.Single() as SimpleBlock);
        }

        private static void Recurse(Core.Picasso picasso, SimpleBlock block)
        {
            RGBA targetColor = picasso.AverageTargetColor(block);

            if (block.Size.X <= MinRes.X && block.Size.Y <= MinRes.Y)
            {
                picasso.Color(block, targetColor);
                Logger.Render(picasso);
                return;
            }

            if (targetColor != block.Color)
            {
                // Get center point of block for cut
                Point blockCenter = new Point(block.BottomLeft.X + block.Size.X / 2, block.BottomLeft.Y + block.Size.Y / 2);
                // Split in 4 and recurse.
                List<SimpleBlock> blocks = picasso.PointCut(block, blockCenter).Cast<SimpleBlock>().ToList();

                foreach(var subBlock in blocks)
                {
                    Recurse(picasso, subBlock);
                }
            }
        }
    }
}