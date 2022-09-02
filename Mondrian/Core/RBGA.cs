namespace Core
{
    public class RGBA
    {
        public int R { get; private set; }
        public int G { get; private set; }
        public int B { get; private set; }
        public int A { get; private set; }

        public RGBA(int r = 0, int g = 0, int b = 0, int a = 0)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}
