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

        public static bool operator == (RGBA a, RGBA b)
        {
            // Perf: use internal Byte instead of property Int
            return (a.r == b.r
                && a.g == b.g
                && a.b == b.b
                && a.a == b.a);
        }

        public static bool operator !=(RGBA a, RGBA b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(r, g, b, a);
        }

        public double Diff(RGBA other)
        {
            // Perf: use internal Byte instead of property Int
            var rDist = (r - other.r) * (r - other.r);
            var gDist = (g - other.g) * (g - other.g);
            var bDist = (b - other.b) * (b - other.b);
            var aDist = (a - other.a) * (a - other.a);
            var distance = Math.Sqrt(rDist + gDist + bDist + aDist);
            return distance;
        }
    }
}
