using System;
using System.Collections;
using System.Collections.Generic;
using PathFinder.Interfaces;

namespace PathFinder.Components
{
    internal class OpenNodeStoreSortedLinkedList<T> : IOpenNodeStore<T> where T : IComparable<T>
    {
        private SortedLinkedListHash<T> _listHash = new SortedLinkedListHash<T>();

        public int Count => _listHash.Count;

        public bool Contains(T node)
        {
            return _listHash.Contains(node);
        }

        public T PopLowest()
        {
            var first = _listHash.First;
            _listHash.RemoveFirst();
            return first.Value;
        }

        public void Add(T node)
        {
            _listHash.Add(node);
        }

        public void Resort(T node)
        {
            _listHash.Remove(node);
            _listHash.Add(node);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _listHash.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _listHash).GetEnumerator();
        }
    }
}