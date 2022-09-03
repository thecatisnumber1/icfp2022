namespace Core
{
    public class InitialBlockDef
    {
        public string blockId;
        public int[] bottomLeft;
        public int[] topRight;
        public byte[] color;
    }

    public class InitialConfig
    {
        public int width;
        public int height;
        public InitialBlockDef[] blocks;
    }
}
