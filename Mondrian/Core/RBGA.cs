namespace Core
{
    public struct RGBA
    {
        private byte r, g, b, a;

        public int R => (int)r;
        public int G => (int)g;
        public int B => (int)b;
        public int A => (int)a;

        public RGBA(int r = 0, int g = 0, int b = 0, int a = 0)
        {
            this.r = (byte)r;
            this.g = (byte)g;
            this.b = (byte)b;
            this.a = (byte)a;
        }

        public override string ToString()
        {
            return $"[{R}, {G}, {B}, {A}]";
        }
    }
}
