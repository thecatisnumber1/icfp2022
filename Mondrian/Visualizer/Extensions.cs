using System;
using Core;

namespace Visualizer
{
    internal static class Extensions
    {
        /// <summary>
        /// Convert a point taken from the viewport to the model, translating screen coordinates to real coordinates
        /// </summary>
        public static Point FromViewportToModel(this System.Windows.Point p, int height)
        {
            return new Point((int)Math.Round(p.X), height - (int)Math.Round(p.Y));
        }
    }
}