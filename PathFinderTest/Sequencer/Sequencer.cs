using System;
using System.Collections.Generic;
using System.Linq;

namespace PathFinderTest.Sequencer
{
    public static class SequenceBuilder
    {
        public static IEnumerable<byte> Build(byte end, byte start = 0, byte inc = 1)
        {
            if (inc == 0) throw new InvalidOperationException("incrementor can not be 0");
            if (end < start) throw new InvalidOperationException("start must be higher than end");
            
            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (current <= end);
        }

        public static IEnumerable<short> Build(short end, short start = 0, short inc = 1)
        {
            if (inc == 0) throw new InvalidOperationException("incrementor can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new InvalidOperationException("invalid incrementor direction");

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (current <= end);
        }

        public static IEnumerable<int> Build(int end, int start = 0, int inc = 1)
        {
            if (inc == 0) throw new InvalidOperationException("incrementor can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new InvalidOperationException("invalid incrementor direction");

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (current <= end);
        }

        public static IEnumerable<float> Build(float end, float start = 0, float inc = 1)
        {
            if (Math.Abs(inc) < float.Epsilon) throw new InvalidOperationException("incrementor can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new InvalidOperationException("invalid incrementor direction");

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (current <= end);
        }

        public static IEnumerable<double> Build(double end, double start = 0, double inc = 1)
        {
            if (Math.Abs(inc) < double.Epsilon) throw new InvalidOperationException("incrementor can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new InvalidOperationException("invalid incrementor direction");

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (current <= end);
        }

        public static IEnumerable<long> Build(long end, long start = 0, long inc = 1)
        {
            if (inc == 0) throw new InvalidOperationException("incrementor can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new InvalidOperationException("invalid incrementor direction");

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (current <= end);
        }

        public static IEnumerable<decimal> Build(decimal end, decimal start = 0, decimal inc = 1)
        {
            if (inc == 0) throw new InvalidOperationException("incrementor can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new InvalidOperationException("invalid incrementor direction");

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (current <= end);
        }

        public static IEnumerable<sbyte> Build(sbyte end, sbyte start = 0, sbyte inc = 1)
        {
            if (inc == 0) throw new InvalidOperationException("incrementor can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new InvalidOperationException("invalid incrementor direction");

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (current <= end);
        }

        public static IEnumerable<double> ToDouble(this IEnumerable<decimal> list) => list.Select(n => (double) n);
    }
}