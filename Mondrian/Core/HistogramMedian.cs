using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class HistogramMedian
    {
        int[] buckets;
        List<int> nonZeroBuckets;

        public HistogramMedian()
        {
            buckets = new int[256];
            nonZeroBuckets = new List<int>(256);
        }

        public void Add(int val)
        {
            if (buckets[val] == 0)
            {
                nonZeroBuckets.Add(val);
            }
            buckets[val]++;
        }

        public int GetMedianAndClear()
        {
            nonZeroBuckets.Sort();
            int totalCount = 0;
            foreach (int i in nonZeroBuckets)
                totalCount += buckets[i];
            int stop = totalCount / 2;

            int count = 0;
            foreach (int i in nonZeroBuckets)
            {
                count += buckets[i];
                if (count > stop)
                {
                    foreach (int j in nonZeroBuckets) buckets[j] = 0;
                    nonZeroBuckets.Clear();
                    return i;
                }
            }
            return 128;
        }
    }
}
