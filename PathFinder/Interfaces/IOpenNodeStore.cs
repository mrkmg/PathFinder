using System;
using System.Collections.Generic;

namespace PathFinder.Interfaces
{
    internal interface IOpenNodeStore<T> : IEnumerable<T> where T : IComparable<T>
    {
        int Count { get; }
        bool Contains(T node);
        T PopLowest();
        void Add(T node);
        void Resort(T node);
    }
}