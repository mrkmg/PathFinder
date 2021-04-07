using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace PathFinder.Components
{
    internal class LinkedList<T> : ICollection<T>
    {
        public int Count { get; private set; }
        public bool IsReadOnly => false;

        [CanBeNull] public LinkedListNode<T> First { get; private set; }
        [CanBeNull] public LinkedListNode<T> Last { get; private set; }
        
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(First);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item) => AddLast(item);

        public LinkedListNode<T> AddFirst(T item)
        {
            var node = new LinkedListNode<T>(item, this)
            {
                Previous = null,
                Next = First
            };
            if (First != null) First.Previous = node;
            Last ??= node;
            First = node;
            Count++;
            return node;
        }

        public LinkedListNode<T> AddLast(T item)
        {
            var node = new LinkedListNode<T>(item, this)
            {
                Previous = Last,
                Next = null
            };
            if (Last != null) Last.Next = node;
            First ??= node;
            Last = node;
            Count++;
            return node;
        }

        public LinkedListNode<T> AddAfter(LinkedListNode<T> existingNode, T item)
        {
            if (existingNode == null) throw new ArgumentException("Can not be null", nameof(existingNode));

            var node = new LinkedListNode<T>(item, this)
            {
                Next = existingNode.Next,
                Previous = existingNode
            };

            if (existingNode.Next != null)
                existingNode.Next.Previous = node;
            else
                Last = node;

            existingNode.Next = node;
            Count++;
            return node;
        }

        public LinkedListNode<T> AddBefore(LinkedListNode<T> existingNode, T item)
        {
            if (existingNode == null) throw new ArgumentException("Can not be null", nameof(existingNode));
            
            var node = new LinkedListNode<T>(item, this)
            {
                Next = existingNode,
                Previous = existingNode.Previous
            };

            if (existingNode.Previous != null)
                existingNode.Previous.Next = node;
            else
                First = node;

            existingNode.Previous = node;
            Count++;
            return node;
        }

        public void Clear()
        {
            if (First == null) return;
            var t = First;
            do
            {
                var c = t;
                t = c.Next;
                c.Invalidate();
            } while (t != null);

            First = null;
            Last = null;
            Count = 0;
        }

        public void MoveBefore(LinkedListNode<T> targetNode, LinkedListNode<T> nodeToMove)
        {
            if (ReferenceEquals(targetNode.Next, nodeToMove) || ReferenceEquals(targetNode.Previous, nodeToMove))
            {
                Swap(targetNode, nodeToMove);
                return;
            }

            if (nodeToMove.Previous == null)
                First = nodeToMove.Next;
            else
                nodeToMove.Previous.Next = nodeToMove.Next;

            if (nodeToMove.Next == null)
                Last = nodeToMove.Previous;
            else
                nodeToMove.Next.Previous = nodeToMove.Previous;

            if (targetNode.Previous == null)
                First = nodeToMove;
            else
                targetNode.Previous.Next = nodeToMove;
            
            nodeToMove.Previous = targetNode.Previous;
            nodeToMove.Next = targetNode;
            targetNode.Previous = nodeToMove;
        }

        public void MoveAfter(LinkedListNode<T> targetNode, LinkedListNode<T> nodeToMove)
        {
            if (ReferenceEquals(targetNode.Next, nodeToMove) || ReferenceEquals(targetNode.Previous, nodeToMove))
            {
                Swap(targetNode, nodeToMove);
                return;
            }
            
            if (nodeToMove.Previous == null)
                First = nodeToMove.Next;
            else
                nodeToMove.Previous.Next = nodeToMove.Next;

            if (nodeToMove.Next == null)
                Last = nodeToMove.Previous;
            else
                nodeToMove.Next.Previous = nodeToMove.Previous;

            if (targetNode.Next == null)
                Last = nodeToMove;
            else
                targetNode.Next.Previous = nodeToMove;
            
            nodeToMove.Next = targetNode.Next;
            nodeToMove.Previous = targetNode;
            targetNode.Next = nodeToMove;
        }

        public void Swap(LinkedListNode<T> node1, LinkedListNode<T> node2)
        {
            if (ReferenceEquals(node1.Next, node2))
            {
                node1.Next = node2.Next;
                node2.Previous = node1.Previous;

                node1.Previous = node2;
                node2.Next = node1;
            }
            else if (ReferenceEquals(node1.Previous, node2))
            {
                node1.Previous = node2.Previous;
                node2.Next = node1.Next;
                
                node1.Next = node2;
                node2.Previous = node1;
            }
            else
            {
                var node1Previous = node1.Previous;
                var node1Next = node1.Next;
                
                node1.Previous = node2.Previous;
                node1.Next = node2.Next;
            
                node2.Previous = node1Previous;
                node2.Next = node1Next;
            }

            if (node1.Previous == null) 
                First = node1;
            else 
                node1.Previous.Next = node1;
            
            if (node1.Next == null) 
                Last = node1;
            else 
                node1.Next.Previous = node1;

            if (node2.Previous == null) 
                First = node2;
            else 
                node2.Previous.Next = node2;
            
            if (node2.Next == null) 
                Last = node2;
            else 
                node2.Next.Previous = node2;
            
        }

        public bool Contains(T item)
        {
            return Find(item) != null;
        }

        public void CopyTo(T[] array, int index)
        {
            if (Count == 0) return;
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (array.Rank != 1) throw new ArgumentException("No Ranked Arrays");
            if (array.GetLowerBound(0) != 0) throw new ArgumentException("Non-Zero Bound", nameof(array));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), index, "Negative Index");
            if (array.Length - index < Count) throw new ArgumentException("Array too small");

            var node = First;
            do
            {
                array[index++] = node.Value;
                node = node.Next;
            } while (node != null);
        }

        public bool Remove(T item)
        {
            var node = Find(item);
            return node != null && Remove(node);
        }

        public bool RemoveFirst()
        {
            switch (Count)
            {
                case 0:
                    return false;
                case 1: 
                    Clear();
                    return true;
                case 2:
                    First.Invalidate();
                    Last.Previous = null;
                    First = Last;
                    Count--;
                    return true;
                default:
                    var fn = First.Next;
                    First.Invalidate();
                    First = fn;
                    First.Previous = null;
                    Count--;
                    return true;
            }
        }

        public bool RemoveLast()
        {
            switch (Count)
            {
                case 0:
                    return false;
                case 1: 
                    Clear();
                    return true;
                case 2:
                    Last.Invalidate();
                    First.Next = null;
                    Last = First;
                    Count--;
                    return true;
                default:
                    var lp = Last.Previous;
                    Last.Invalidate();
                    Last = Last.Previous;
                    Last.Previous = lp;
                    Count--;
                    return true;
            }
        }

        public bool Remove(LinkedListNode<T> node)
        {
            if (node.List != this) return false;

            if (node.Next == null) Last = node.Previous;
            else node.Next.Previous = node.Previous;

            if (node.Previous == null) First = node.Next;
            else node.Previous.Next = node.Next;
            
            node.Invalidate();
            
            Count--;
            return true;
        }
        
        public LinkedListNode<T> Find(T val)
        {
            var comp = EqualityComparer<T>.Default;
            return Find((t) => comp.Equals(val, t));
        }

        public LinkedListNode<T> Find(Func<T, bool> predicate)
        {
            if (Count == 0) return null;
            if (Count == 1) return First;

            var c = First;
            while (c != null && !predicate(c.Value)) c = c.Next;
            return c;
        }

        public LinkedListNode<T> FindReverse(T val)
        {
            var comp = EqualityComparer<T>.Default;
            return FindReverse((t) => comp.Equals(val, t));
        }

        public LinkedListNode<T> FindReverse(Func<T, bool> predicate)
        {
            if (Count == 0) return null;
            if (Count == 1) return Last;

            var c = Last;
            while (c != null && !predicate(c.Value)) c = c.Previous;
            return c;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly LinkedListNode<T> _start;
            private readonly bool _forwards;
            private LinkedListNode<T> _next;
            private LinkedListNode<T> _current;

            public Enumerator(LinkedListNode<T> start, bool forwards = true)
            {
                _start = start;
                _current = default;
                _forwards = forwards;
                _next = _start ?? null;
            }

            public bool MoveNext()
            {
                if (_next == null) return false;
                _current = _next;
                _next = _forwards ? _current.Next : _current.Previous;
                return true;
            }

            public void Reset()
            {
                _current = default;
                _next = _start;
            }

            public T Current => _current!.Value;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
    
    [DebuggerDisplay("Node {Value.ToString()}")]
    internal class LinkedListNode<T>
    {
        public LinkedListNode<T> Previous { get; set; }
        public LinkedListNode<T> Next { get; set; }
        public T Value;
        public LinkedList<T> List;
            
        public LinkedListNode(T val, LinkedList<T> list)
        {
            Value = val;
            List = list;
        }

        public override string ToString()
        {
            return $"Node {Value.ToString()}";
        }

        public void Invalidate()
        {
            Previous = null;
            Next = null;
            List = null;
        }
    }

}