using System;
using System.Collections;
using System.Collections.Generic;

namespace PathFinder.Components
{
    internal class SortedLinkedListHash<T> : ICollection<T> where T : IComparable<T>
    {
        private readonly LinkedList<T> _list = new LinkedList<T>();
        private readonly Dictionary<T, LinkedListNode<T>> _listNodes = new Dictionary<T, LinkedListNode<T>>(); 

        public void Add(T value)
        {
            if (value == null) return;
            
            var first = _list.First;
            var last = _list.Last;
            
            if (Count == 0 || value.CompareTo(first.Value) < 0)
            {
                AddFirst(value);
                return;
            }

            if (value.CompareTo(last.Value) > 0)
            {
                AddLast(value);
                return;
            }

            while (true)
            {
                if (value.CompareTo(first.Value) <= 0)
                {
                    AddBefore(first, value);
                    return;
                }

                if (value.CompareTo(last.Value) >= 0)
                {
                    AddAfter(last, value);
                    return;
                }

                first = first.Next;
                last = last.Previous;
            }
        }

        private void AddFirst(T value)
        {
            _listNodes[value] = _list.AddFirst(value);
        }

        private void AddLast(T value)
        {
            _listNodes[value] = _list.AddLast(value);
        }

        private void AddBefore(LinkedListNode<T> node, T value)
        {
            _listNodes[value] = _list.AddBefore(node, value);
        }

        private void AddAfter(LinkedListNode<T> node, T value)
        {
            _listNodes[value] = _list.AddAfter(node, value);
        }

        public void Clear()
        {
            _list.Clear();
            _listNodes.Clear();
        }

        public bool Contains(T item)
        {
            return item != null && _listNodes.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (item == null || !_listNodes.ContainsKey(item)) return false;
            
            _list.Remove(_listNodes[item]);
            _listNodes.Remove(item);
            
            return true;
        }

        public void RemoveFirst()
        {
            _listNodes.Remove(_list.First.Value);
            _list.RemoveFirst();
        }

        public void RemoveLast()
        {
            _listNodes.Remove(_list.Last.Value);
            _list.RemoveLast();
        }

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