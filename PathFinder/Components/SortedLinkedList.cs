using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace PathFinder.Components
{
    internal class SortedLinkedList<T> : ICollection<T>
    {
        private readonly LinkedList<T> _list = new();
        private readonly Dictionary<T, LinkedListNode<T>> _listNodes = new();
        private readonly Comparer<T> _comparer;

        public SortedLinkedList(Comparer<T> comparer)
        {
            _comparer = comparer;
        }

        public void Resort([CanBeNull] T value)
        {
            if (value == null) return;
            if (!_listNodes.ContainsKey(value)) return;

            var node = _listNodes[value];

            if (node.Previous != null && _comparer.Compare(value, node.Previous.Value) < 0)
            {
                var cNode = node.Previous;
                while (cNode.Previous != null && _comparer.Compare(value, cNode.Previous.Value) < 0)
                {
                    cNode = cNode.Previous;
                }
                _list.MoveBefore(cNode, node);
            }
            else if (node.Next != null && _comparer.Compare(value, node.Next.Value) > 0)
            {
                var cNode = node.Next;
                while (cNode.Next != null && _comparer.Compare(value, cNode.Next.Value) > 0)
                {
                    cNode = cNode.Next;
                }
                _list.MoveAfter(cNode, node);
            }
        }
        
        public void Add(T value)
        {
            if (value == null) return;
            
            var first = _list.First;
            var last = _list.Last;
            
            // ReSharper disable once PossibleNullReferenceException
            // Count == 0 prevents first from being null
            if (Count == 0 || _comparer.Compare(value, first.Value) < 0)
            {
                AddFirst(value);
                return;
            }

            // ReSharper disable once PossibleNullReferenceException
            // Count == 0 prevents first from being null
            if (_comparer.Compare(value, last.Value) > 0)
            {
                AddLast(value);
                return;
            }

            while (true)
            {
                if (_comparer.Compare(value, first.Value) <= 0)
                {
                    AddBefore(first, value);
                    return;
                }

                if (_comparer.Compare(value, last.Value) >= 0)
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
        
        public T PopFirst()
        {
            Debug.Assert(_list.First != null);
            var value = _list.First.Value;
            RemoveFirst();
            return value;
        }

        public T PopLast()
        {
            Debug.Assert(_list.Last != null);
            var value = _list.Last.Value;
            RemoveLast();
            return value;
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
            Debug.Assert(_list.First != null);
            _listNodes.Remove(_list.First.Value);
            _list.RemoveFirst();
        }

        public void RemoveLast()
        {
            Debug.Assert(_list.Last != null);
            _listNodes.Remove(_list.Last.Value);
            _list.RemoveLast();
        }

        [CanBeNull] public LinkedListNode<T> First => _list.First;
        [CanBeNull] public LinkedListNode<T> Last => _list.Last;

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