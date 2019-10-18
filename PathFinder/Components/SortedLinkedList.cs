using System;
using System.Collections;
using System.Collections.Generic;

namespace PathFinder.Components
{
    internal class SortedLinkedList<T> : ICollection<T>
    {
        private readonly Comparison<T> _comparer;
        private LinkedList<T> _list = new LinkedList<T>();
        
        public SortedLinkedList(Comparison<T> comparer)
        {
            _comparer = comparer;
        }

        public void Add(T value)
        {
            var first = _list.First;
            var last = _list.Last;
            
            if (Count == 0 || _comparer(value, first.Value) <= 0)
            {
                _list.AddFirst(value);
                return;
            }

            if (_comparer(value, last.Value) >= 0)
            {
                _list.AddLast(value);
                return;
            }


            while (true)
            {
                if (_comparer(value, first.Value) <= 0)
                {
                    _list.AddBefore(first, value);
                    return;
                }

                if (_comparer(value, last.Value) >= 0)
                {
                    _list.AddAfter(last, value);
                    return;
                }

                first = first.Next;
                last = last.Previous;
            }
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public void RemoveFirst() => _list.RemoveFirst();
        public void RemoveLast() => _list.RemoveLast();

        public LinkedListNode<T> First => _list.First;
        public LinkedListNode<T> Last => _list.Last;

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _list).GetEnumerator();
        }
    }
}