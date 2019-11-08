using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Interfaces;

namespace PathFinder.Components
{
    internal class OpenNodeStoreHashSet<T> : IOpenNodeStore<T> where T : IComparable<T>
    {
        private readonly HashSet<T> _hash = new HashSet<T>();

        public int Count => _hash.Count;
        
        public bool Contains(T node)
        {
            return _hash.Contains(node);
        }

        public T PopLowest()
        {
            var lowest = _hash.Aggregate((a, b) => a.CompareTo(b) < 0 ? a : b);
            _hash.Remove(lowest);
            return lowest;
        }

        public void Add(T node)
        {
            _hash.Add(node);
        }

        public void Resort(T node)
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _hash.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _hash).GetEnumerator();
        }
    }
}