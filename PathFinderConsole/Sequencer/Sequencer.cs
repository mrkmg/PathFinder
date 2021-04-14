using System.Collections.Generic;

// ReSharper disable once CheckNamespace
// Extending enumerable to add Sequence
namespace System.Linq
{
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<sbyte> Sequence(sbyte end) => Sequence((sbyte)0, end, (sbyte)1);
        public static IEnumerable<sbyte> Sequence(sbyte start, sbyte end) => Sequence(start, end, start < end ? (sbyte)1 : (sbyte)-1);
        public static IEnumerable<sbyte> Sequence(sbyte start, sbyte end, sbyte inc)
        {
            if (inc == 0) throw new ArgumentOutOfRangeException(nameof(inc), inc, "can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new ArgumentException("invalid incrementer direction", nameof(inc));

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (inc > 0 ? current <= end : current >= end);
        }
        
        public static IEnumerable<byte> Sequence(byte end) => Sequence((byte)0, end, (byte)1);
        public static IEnumerable<byte> Sequence(byte start, byte end) => Sequence(start, end, (byte)1);
        public static IEnumerable<byte> Sequence(byte start, byte end, byte inc)
        {
            if (inc == 0) throw new ArgumentOutOfRangeException(nameof(inc), inc, "can not be 0");
            if (end < start) throw new ArgumentException("must be greater than start", nameof(end));
            
            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (current <= end);
        }

        public static IEnumerable<short> Sequence(short end) => Sequence((short)0, end, (short)1);
        public static IEnumerable<short> Sequence(short start, short end) => Sequence(start, end, start < end ? (short)1 : (short)-1);
        public static IEnumerable<short> Sequence(short start, short end, short inc)
        {
            if (inc == 0) throw new ArgumentOutOfRangeException(nameof(inc), inc, "can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new ArgumentException("invalid incrementer direction", nameof(inc));

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (inc > 0 ? current <= end : current >= end);
        }
        
        public static IEnumerable<ushort> Sequence(ushort end) => Sequence((ushort)0, end, (ushort)1);
        public static IEnumerable<ushort> Sequence(ushort start, ushort end) => Sequence(start, end, (ushort)1);
        public static IEnumerable<ushort> Sequence(ushort start, ushort end, ushort inc)
        {
            if (inc == 0) throw new ArgumentOutOfRangeException(nameof(inc), inc, "can not be 0");
            if (end < start) throw new ArgumentException("must be greater than start", nameof(end));
            
            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (current <= end);
        }

        public static IEnumerable<int> Sequence(int end) => Sequence(0, end, 1);
        public static IEnumerable<int> Sequence(int start, int end) => Sequence(start, end, start < end ? 1 : -1);
        public static IEnumerable<int> Sequence(int start, int end, int inc)
        {
            if (inc == 0) throw new ArgumentOutOfRangeException(nameof(inc), inc, "can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new ArgumentException("invalid incrementer direction", nameof(inc));

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (inc > 0 ? current <= end : current >= end);
        }
        
        public static IEnumerable<uint> Sequence(uint end) => Sequence(0, end, 1);
        public static IEnumerable<uint> Sequence(uint start, uint end) => Sequence(start, end, 1);
        public static IEnumerable<uint> Sequence(uint start, uint end, uint inc)
        {
            if (inc == 0) throw new ArgumentOutOfRangeException(nameof(inc), inc, "can not be 0");
            if (end < start) throw new ArgumentException("must be greater than start", nameof(end));
            
            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (inc > 0 ? current <= end : current >= end);
        }

        public static IEnumerable<long> Sequence(long end) => Sequence(0, end, 1);
        public static IEnumerable<long> Sequence(long start, long end) => Sequence(start, end, start < end ? 1 : -1);
        public static IEnumerable<long> Sequence(long start, long end, long inc)
        {
            if (inc == 0) throw new ArgumentOutOfRangeException(nameof(inc), inc, "can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new ArgumentException("invalid incrementer direction", nameof(inc));

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (inc > 0 ? current <= end : current >= end);
        }
        
        public static IEnumerable<ulong> Sequence(ulong end) => Sequence(0, end, 1);
        public static IEnumerable<ulong> Sequence(ulong start, ulong end) => Sequence(start, end, 1);
        public static IEnumerable<ulong> Sequence(ulong start, ulong end, ulong inc)
        {
            if (inc == 0) throw new ArgumentOutOfRangeException(nameof(inc), inc, "can not be 0");
            if (end < start) throw new ArgumentException("must be greater than start", nameof(end));
            
            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (inc > 0 ? current <= end : current >= end);
        }

        public static IEnumerable<float> Sequence(float end) => Sequence(0, end, 1);
        public static IEnumerable<float> Sequence(float start, float end) => Sequence(start, end, 1);
        public static IEnumerable<float> Sequence(float start, float end, float inc)
        {
            if (inc == 0) throw new ArgumentOutOfRangeException(nameof(inc), inc, "can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new ArgumentException("invalid incrementer direction", nameof(inc));

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (inc > 0 ? current <= end : current >= end);
        }

        public static IEnumerable<double> Sequence(double end) => Sequence(0, end, 1);
        public static IEnumerable<double> Sequence(double start, double end) => Sequence(start, end, 1);
        public static IEnumerable<double> Sequence(double start, double end, double inc)
        {
            if (inc == 0) throw new ArgumentOutOfRangeException(nameof(inc), inc, "can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new ArgumentException("invalid incrementer direction", nameof(inc));

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (inc > 0 ? current <= end : current >= end);
        }

        public static IEnumerable<decimal> Sequence(decimal end) => Sequence(0, end, 1);
        public static IEnumerable<decimal> Sequence(decimal start, decimal end) => Sequence(start, end, 1);
        public static IEnumerable<decimal> Sequence(decimal start, decimal end, decimal inc)
        {
            if (inc == 0) throw new ArgumentOutOfRangeException(nameof(inc), inc, "can not be 0");
            if (start < end && inc < 0 || start > end && inc > 0) throw new ArgumentException("invalid incrementer direction", nameof(inc));

            var current = start;

            do
            {
                yield return current;
                current += inc;
            } while (inc > 0 ? current <= end : current >= end);
        }
    }
}