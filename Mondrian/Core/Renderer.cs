using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Renderer
    {
        // Was Painter.draw(canvas)
        public static RGBA[] CanvasToFrame(Canvas canvas)
        {
            var blocks = canvas.Simplify();
            var frame = new RGBA[canvas.Width * canvas.Height];
            int size = 0;

            foreach (var block in blocks)
            {
                var frameTopLeft = new Point(block.BottomLeft.X, canvas.Height - block.TopRight.Y);
                var frameBottomRight = new Point(block.TopRight.X, canvas.Height - block.BottomLeft.Y);
                size += (frameBottomRight.X - frameTopLeft.X) * (frameBottomRight.Y - frameTopLeft.Y);

                for (var y = frameTopLeft.Y; y < frameBottomRight.Y; y++)
                {
                    for (var x = frameTopLeft.X; x < frameBottomRight.X; x++)
                    {
                        frame[y * canvas.Width + x] = block.Color;
                    }
                }
            }

            return frame;
        }

    }
}
