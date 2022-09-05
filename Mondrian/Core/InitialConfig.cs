﻿namespace Core
{
    public class InitialBlockDef
    {
        public string blockId { get; set; }
        public int[] bottomLeft { get; set; }
        public int[] topRight { get; set; }

        // One or the other
        public int[] color { get; set; }
        public int[] pngBottomLeftPoint { get; set; }
    }

    public class InitialConfig
    {
        public int width { get; set; }
        public int height { get; set; }
        public InitialBlockDef[] blocks { get; set; }
    }
}
