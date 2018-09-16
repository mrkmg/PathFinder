using System;
using System.Collections;
using System.Collections.Generic;

namespace PathFinderTest.Sequencer
{
    public static class SequenceBuilder
    {
        public static IEnumerable<short> Build(short end, short start = 0, short inc = 1) => new SequencerShort(start, end, inc);
        public static IEnumerable<int> Build(int end, int start = 0, int inc = 1) => new SequencerInt(start, end, inc);
        public static IEnumerable<float> Build(float end, float start = 0, float inc = 1) => new SequencerFloat(start, end, inc);
        public static IEnumerable<long> Build(long end, long start, long inc = 1) => new SequencerLong(end, start, inc);
        public static IEnumerable<double> Build(double end, double start = 1, double inc = 1) => new SequencerDouble(start, end, inc);
        public static IEnumerable<decimal> Build(decimal end, decimal start = 1, decimal inc = 1) => new SequencerDecimal(start, end, inc);
    }

    public class SequencerShort : IEnumerable<short>
    {
        private readonly short _start;
        private readonly short _end;
        private readonly short _inc;

        public SequencerShort(short start, short end, short inc)
        {
            if (inc == 0) throw new InvalidOperationException("incrementor can not be 0");
            _start = start;
            _end = end;
            _inc = inc;
        }

        public IEnumerator<short> GetEnumerator() => new SequencerEnumeratorShort(_start, _end, _inc);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class SequencerEnumeratorShort : IEnumerator<short>
    {
        private readonly short _start;
        private readonly short _end;
        private readonly short _inc;
        private bool _isFirst;
        public short Current { get; private set; }

        public SequencerEnumeratorShort(short start, short end, short inc)
        {
            _start = start;
            _end = end;
            _inc = inc;
            Current = _start;
        }

        public bool MoveNext()
        {
            if (_isFirst)
            {
                _isFirst = false;
                return true;
            }

            if (_inc < 0 && Current <= _end || _inc > 0 && Current >= _end) return false;
            Current += _inc;
            return true;
        }

        public void Reset()
        {
            Current = _start;
            _isFirst = false;
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }

    public class SequencerInt : IEnumerable<int>
    {
        private readonly int _start;
        private readonly int _end;
        private readonly int _inc;

        public SequencerInt(int start, int end, int inc)
        {
            if (inc == 0) throw new InvalidOperationException("incrementor can not be 0");
            _start = start;
            _end = end;
            _inc = inc;
        }

        public IEnumerator<int> GetEnumerator() => new SequencerEnumeratorInt(_start, _end, _inc);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class SequencerEnumeratorInt : IEnumerator<int>
    {
        private readonly int _start;
        private readonly int _end;
        private readonly int _inc;
        private bool _isFirst;
        public int Current { get; private set; }

        public SequencerEnumeratorInt(int start, int end, int inc)
        {
            _start = start;
            _end = end;
            _inc = inc;
            Current = _start;
        }

        public bool MoveNext()
        {
            if (_isFirst)
            {
                _isFirst = false;
                return true;
            }

            if (_inc < 0 && Current <= _end || _inc > 0 && Current >= _end) return false;
            Current += _inc;
            return true;
        }

        public void Reset()
        {
            Current = _start;
            _isFirst = false;
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }

    public class SequencerFloat : IEnumerable<float>
    {
        private readonly float _start;
        private readonly float _end;
        private readonly float _inc;

        public SequencerFloat(float start, float end, float inc)
        {
            if (Math.Abs(inc) < float.Epsilon) throw new InvalidOperationException("incrementor can not be 0");
            _start = start;
            _end = end;
            _inc = inc;
        }

        public IEnumerator<float> GetEnumerator() => new SequencerEnumeratorFloat(_start, _end, _inc);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class SequencerEnumeratorFloat : IEnumerator<float>
    {
        private readonly float _start;
        private readonly float _end;
        private readonly float _inc;
        private bool _isFirst;
        public float Current { get; private set; }

        public SequencerEnumeratorFloat(float start, float end, float inc)
        {
            _start = start;
            _end = end;
            _inc = inc;
            Current = _start;
        }

        public bool MoveNext()
        {
            if (_isFirst)
            {
                _isFirst = false;
                return true;
            }

            if (_inc < 0 && Current <= _end || _inc > 0 && Current >= _end) return false;
            Current += _inc;
            return true;
        }

        public void Reset()
        {
            Current = _start;
            _isFirst = false;
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }

    public class SequencerLong : IEnumerable<long>
    {
        private readonly long _start;
        private readonly long _end;
        private readonly long _inc;

        public SequencerLong(long start, long end, long inc)
        {
            if (inc == 0) throw new InvalidOperationException("incrementor can not be 0");
            _start = start;
            _end = end;
            _inc = inc;
        }

        public IEnumerator<long> GetEnumerator() => new SequencerEnumeratorLong(_start, _end, _inc);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class SequencerEnumeratorLong : IEnumerator<long>
    {
        private readonly long _start;
        private readonly long _end;
        private readonly long _inc;
        private bool _isFirst;
        public long Current { get; private set; }

        public SequencerEnumeratorLong(long start, long end, long inc)
        {
            _start = start;
            _end = end;
            _inc = inc;
            Current = _start;
        }

        public bool MoveNext()
        {
            if (_isFirst)
            {
                _isFirst = false;
                return true;
            }

            if (_inc < 0 && Current <= _end || _inc > 0 && Current >= _end) return false;
            Current += _inc;
            return true;
        }

        public void Reset()
        {
            Current = _start;
            _isFirst = false;
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }

    public class SequencerDouble : IEnumerable<double>
    {
        private readonly double _start;
        private readonly double _end;
        private readonly double _inc;

        public SequencerDouble(double start, double end, double inc)
        {
            if (Math.Abs(inc) < double.Epsilon) throw new InvalidOperationException("incrementor can not be 0");
            _start = start;
            _end = end;
            _inc = inc;
        }

        public IEnumerator<double> GetEnumerator() => new SequencerEnumeratorDouble(_start, _end, _inc);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class SequencerEnumeratorDouble : IEnumerator<double>
    {
        private readonly double _start;
        private readonly double _end;
        private readonly double _inc;
        private bool _isFirst;
        public double Current { get; private set; }

        public SequencerEnumeratorDouble(double start, double end, double inc)
        {
            _start = start;
            _end = end;
            _inc = inc;
            Current = _start;
        }

        public bool MoveNext()
        {
            if (_isFirst)
            {
                _isFirst = false;
                return true;
            }

            if (_inc < 0 && Current <= _end || _inc > 0 && Current >= _end) return false;
            Current += _inc;
            return true;
        }

        public void Reset()
        {
            Current = _start;
            _isFirst = false;
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }

    public class SequencerDecimal : IEnumerable<decimal>
    {
        private readonly decimal _start;
        private readonly decimal _end;
        private readonly decimal _inc;

        public SequencerDecimal(decimal start, decimal end, decimal inc)
        {
            if (inc == 0) throw new InvalidOperationException("incrementor can not be 0");
            _start = start;
            _end = end;
            _inc = inc;
        }

        public IEnumerator<decimal> GetEnumerator() => new SequencerEnumeratorDecimal(_start, _end, _inc);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class SequencerEnumeratorDecimal : IEnumerator<decimal>
    {
        private readonly decimal _start;
        private readonly decimal _end;
        private readonly decimal _inc;
        private bool _isFirst;
        public decimal Current { get; private set; }

        public SequencerEnumeratorDecimal(decimal start, decimal end, decimal inc)
        {
            _start = start;
            _end = end;
            _inc = inc;
            Current = _start;
        }

        public bool MoveNext()
        {
            if (_isFirst)
            {
                _isFirst = false;
                return true;
            }

            if (_inc < 0 && Current <= _end || _inc > 0 && Current >= _end) return false;
            Current += _inc;
            return true;
        }

        public void Reset()
        {
            Current = _start;
            _isFirst = true;
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}
