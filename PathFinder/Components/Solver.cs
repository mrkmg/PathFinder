using System;
using System.Collections.Generic;
using PathFinder.Interfaces;

namespace PathFinder.Components
{
    public abstract class Solver<T> : ISolver<T> where T : INode
    {
        /// <summary>
        /// The current state of the solver.
        /// </summary>
        public SolverState State { get; protected set; } = SolverState.Running;

        /// <summary>
        /// The number of ticks this solver has performed.
        /// </summary>
        public int Ticks { get; protected set; }

        /// <summary>
        /// A reference to the origin.
        /// </summary>
        public T Origin => OriginNode.Obj;

        /// <summary>
        /// A reference to the destination.
        /// </summary>
        public T Destination => DestinationNode.Obj;

        public IList<T> Path
        {
            get => GetPath();
            private set => _path = value;
        }

        /// <summary>
        /// A reference to the metadata of the origin.
        /// </summary>
        protected readonly Node<T> OriginNode;

        /// <summary>
        /// A reference to the metadata of the destination.
        /// </summary>
        protected readonly Node<T> DestinationNode;

        private readonly Dictionary<T, Node<T>> _nodeLookup = new Dictionary<T, Node<T>>();
        private IList<T> _path;

        protected Solver(T origin, T destination)
        {
            DestinationNode = GetNode(destination, false);
            OriginNode = GetNode(origin);
        }

        /// <summary>
        /// Cancel the current solver.
        /// </summary>
        public void Cancel()
        {
            State = SolverState.Failed;
        }

        /// <summary>
        /// Perform a tick.
        /// </summary>
        public abstract void Tick();

        protected abstract IList<T> BuildPath();

        protected Node<T> GetNode(T obj) => GetNode(obj, true);
        protected Node<T> GetNode(T obj, bool performToCost)
        {
            if (_nodeLookup.ContainsKey(obj))
            {
                return _nodeLookup[obj];
            }

            var node = new Node<T>(obj);
            _nodeLookup.Add(obj, node);
            node.FromCost = 0;
            node.ToCost = performToCost ? node.EstimatedCostTo(DestinationNode) : 0;
            node.TotalCost = node.FromCost + node.ToCost;
            return node;
        }

        private IList<T> GetPath()
        {
            if (State != SolverState.Success) return null;
            return _path ?? (_path = BuildPath());
        }
    }
}
