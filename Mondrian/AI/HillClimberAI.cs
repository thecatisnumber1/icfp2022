using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace AI
{
    public class HillClimberAI
    {
        public static void Solve(Picasso picasso, AIArgs args, LoggerBase logger)
        {
            PlaceAllRectangles(picasso, new List<Rectangle> { new Rectangle(new Point(25, 25), new Point(300, 300)) }, logger);
            logger.Render(picasso);
        }

        public static void PlaceAllRectangles(Picasso picasso, List<Rectangle> rects, LoggerBase logger)
        {
            foreach (Rectangle rect in rects)
            {
                PlaceRectangle(picasso, rect);
            }
        }

        public static void PlaceRectangle(Picasso picasso, Rectangle rect)
        {
            if (picasso.BlockCount > 1)
            {
                throw new Exception("Can't place a rectangle on a complex canvas!");
            }

            List<Block> blocks0 = picasso.PointCut(picasso.AllBlocks.First().ID, rect.BottomLeft).ToList(); ;
            List<Block> blocks1 = picasso.PointCut(blocks0[2].ID, rect.TopRight).ToList();
            picasso.Color(blocks1[0].ID, picasso.AverageTargetColor(blocks1[0]));
        }
    }
}
