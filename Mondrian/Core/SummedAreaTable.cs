using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class SummedAreaTable<E>
    {
        private readonly IMath<E> math;
        private readonly E[,] table;

        public SummedAreaTable(E[,] values, IMath<E> math)
        {
            this.math = math;
            int width = values.GetLength(0);
            int height = values.GetLength(1);

            table = new E[width + 1, height + 1];

            for (int y = 0; y < height; y++)
            {
                E rowSum = math.Zero();
                for (int x = 0; x < width; x++)
                {
                    rowSum = math.Add(rowSum, values[x, y]);
                    table[x + 1, y + 1] = math.Add(table[x + 1, y], rowSum);
                }
            }
        }

        public E GetSum(Rectangle rect)
        {
            return math.Sub(math.Sub(table[rect.Right, rect.Top], table[rect.Left, rect.Top]),
                math.Sub(table[rect.Right, rect.Bottom], table[rect.Left, rect.Bottom]));
        }
    }


    public interface IMath<E>
    {
        public E Zero();
        public E Add(E e1, E e2);
        public E Sub(E e1, E e2);
    }

    public class DoubleMath : IMath<double>
    {
        public double Add(double e1, double e2)
        {
            return e1 + e2;
        }

        public double Sub(double e1, double e2)
        {
            return e1 - e2;
        }

        public double Zero()
        {
            return 0.0;
        }
    }
}
