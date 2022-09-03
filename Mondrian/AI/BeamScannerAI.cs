using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace AI
{
    public class BeamScannerAI
    {
        public static readonly int BEAM_WIDTH = 20;

        public static void Solve(Picasso picasso, AIArgs args, LoggerBase logger)
        {
            picasso.Color(picasso.AllBlocks.First().ID, picasso.AverageTargetColor(picasso.AllBlocks.First()));
            logger.Render(picasso);
            ScanBlock(picasso, picasso.AllBlocks.First(), logger, true);
            int scannerScore = picasso.Score;
            picasso.Undo(picasso.InstructionCount);
            ScannerAI.Solve(picasso, args, new ConsoleLogger());
            logger.LogMessage($"BeamScanner score = {scannerScore}, Scanner score = {picasso.Score}.");
            logger.Render(picasso);
        }

        public static int ScanBlock(Picasso picasso, Block block, LoggerBase logger, bool commit)
        {
            var pq = new PriorityQueue<(bool verticalBest, int index), int>();
            int bestScore = picasso.Score;
            for (int x = block.BottomLeft.X + 1; x < block.TopRight.X - 1; x++)
            {
                List<Block> blocks = picasso.VerticalCut(block.ID, x).ToList();
                picasso.Color(blocks[0].ID, picasso.AverageTargetColor(blocks[0]));
                picasso.Color(blocks[1].ID, picasso.AverageTargetColor(blocks[1]));
                if (picasso.Score < bestScore)
                {
                    bestScore = picasso.Score;
                }
                if (pq.Count < BEAM_WIDTH)
                {
                    pq.Enqueue((true, x), -picasso.Score);
                }
                else
                {
                    pq.EnqueueDequeue((true, x), -picasso.Score);
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
                    bestScore = picasso.Score;
                }
                if (pq.Count < BEAM_WIDTH)
                {
                    pq.Enqueue((false, y), -picasso.Score);
                }
                else
                {
                    pq.EnqueueDequeue((false, y), -picasso.Score);
                }

                picasso.Undo(3);
            }

            if (bestScore >= picasso.Score || !commit)
            {
                return bestScore;
            }

            bestScore = picasso.Score;
            bool verticalBest = false;
            int index = -1;
            while (pq.Count > 0)
            {
                var current = pq.Dequeue();
                List<Block> bs;
                if (current.verticalBest)
                {
                    bs = picasso.VerticalCut(block.ID, current.index).ToList();
                }
                else
                {
                    bs = picasso.HorizontalCut(block.ID, current.index).ToList();
                }

                picasso.Color(bs[0].ID, picasso.AverageTargetColor(bs[0]));
                picasso.Color(bs[1].ID, picasso.AverageTargetColor(bs[1]));

                int scoreDiff = ScanBlock(picasso, bs[0], logger, false) - picasso.Score;
                scoreDiff += ScanBlock(picasso, bs[1], logger, false) - picasso.Score;
                picasso.Undo(3);

                if (scoreDiff < bestScore)
                {
                    verticalBest = current.verticalBest;
                    index = current.index;
                    bestScore = scoreDiff;
                }
            }

            List<Block> nextBlocks;
            if (verticalBest)
            {
                nextBlocks = picasso.VerticalCut(block.ID, index).ToList();
            }
            else
            {
                nextBlocks = picasso.HorizontalCut(block.ID, index).ToList();
            }

            picasso.Color(nextBlocks[0].ID, picasso.AverageTargetColor(nextBlocks[0]));
            picasso.Color(nextBlocks[1].ID, picasso.AverageTargetColor(nextBlocks[1]));
            logger.Render(picasso);
            
            ScanBlock(picasso, nextBlocks[0], logger, true);
            ScanBlock(picasso, nextBlocks[1], logger, true);
            return picasso.Score;
        }
    }
}
