using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace AI
{
    public class AIUtils
    {
        public static void RejoinAll(Picasso picasso)
        {
            // Assume square.  Assume equal size.
            Block first = picasso.AllBlocks.First();
            int size = first.TopRight.X - first.BottomLeft.X;
            if (400 % size != 0)
            {
                throw new Exception("I didn' think this could be true.");
            }

            Block[,] blocks = new Block[400 / size, 400 / size];

            foreach (Block block in picasso.AllBlocks)
            {
                blocks[block.BottomLeft.X / size, block.BottomLeft.Y / size] = block;
            }

            for (int row = 0; row < 400 / size; row++)
            {
                for (int col = 0; col < 400 / size - 1; col++)
                {
                    blocks[0, row] = picasso.Merge(blocks[0, row].ID, blocks[col + 1, row].ID);
                }

                if (row > 0)
                {
                    blocks[0, 0] = picasso.Merge(blocks[0, 0].ID, blocks[0, row].ID);
                }
            }
        }
    }
}
