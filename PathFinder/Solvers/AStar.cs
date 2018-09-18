using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Components;
using PathFinder.Interfaces;

namespace PathFinder.Solvers
{
    public class AStar<T> : ISolver<T> where T : INode
    {
        private readonly HashSet<T> _closedNodes = new HashSet<T>();
        private readonly NodeMetas<T> _nodeMetas = new NodeMetas<T>();
        private readonly HashSet<T> _openNodes = new HashSet<T>();
        private T _currentNode;
        private IList<T> _path;
        private IEnumerable<T> _pendingNeighbors;
        private double _thoroughness = 0.5;

        /// <summary>
        ///     Creates a solver using the A* method
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="destination">The destination</param>
        public AStar(T origin, T destination)
        {
            Origin = origin;
            Destination = destination;
            _openNodes.Add(origin);
            _currentNode = Origin;
        }

        /// <summary>
        ///     A List of nodes which still need to be checked
        /// </summary>
        public IEnumerable<T> Open => _openNodes.AsEnumerable();

        /// <summary>
        ///     A list of nodes which have already been checked
        /// </summary>
        public IEnumerable<T> Closed => _closedNodes.AsEnumerable();

        /// <summary>
        ///     The last node to be checked
        /// </summary>
        public T Current => _currentNode;

        /// <summary>
        ///     How thorough the search should be. Should be a value between 0 and 1.
        /// </summary>
        public double Thoroughness
        {
            get => _thoroughness;
            set
            {
                if (value < 0 || value > 1) throw new Exception("Invalid Value");
                _thoroughness = value;
            }
        }

        /// <summary>
        ///     The cost to get from the Origin to the Destination via the path
        /// </summary>
        public double Cost => CalcCost();

        /// <summary>
        ///     The current state of the solver.
        /// </summary>
        public SolverState State { get; private set; } = SolverState.Running;

        /// <summary>
        ///     The number of ticks this solver has performed.
        /// </summary>
        public int Ticks { get; private set; }

        /// <summary>
        ///     A reference to the origin.
        /// </summary>
        public T Origin { get; }

        /// <summary>
        ///     A reference to the destination.
        /// </summary>
        public T Destination { get; }

        /// <summary>
        ///     The path from the Origin to the Destination
        /// </summary>
        public IList<T> Path => GetPath();

        /// <summary>
        ///     Perform one tick
        /// </summary>
        public void Tick()
        {
            if (State != SolverState.Running) throw new Exception("Not running");

            if (_openNodes.Count == 0)
            {
                State = SolverState.Failed;
                return;
            }

            ProcessNeighbors();

            Ticks++;

            if (_currentNode.Equals(Destination))
                State = SolverState.Success;
            else if (_openNodes.Count == 0)
                State = SolverState.Failed;
            else
                SetCurrentNode();
        }

        /// <summary>
        ///     Cancel the current solver.
        /// </summary>
        public void Cancel()
        {
            State = SolverState.Failed;
        }

        private static NodeMetaData<T> LowestNodeAggregate(NodeMetaData<T> lNode, NodeMetaData<T> tNode)
        {
            return Math.Abs(lNode.TotalCost - tNode.TotalCost) < 0.1 ? lNode.ToCost < tNode.ToCost ? lNode : tNode :
                lNode.TotalCost < tNode.TotalCost ? lNode : tNode;
        }

        private void SetCurrentNode()
        {
            _currentNode = _openNodes.Select(_nodeMetas.Get).Aggregate(LowestNodeAggregate).Obj;

            _openNodes.Remove(_currentNode);
            _closedNodes.Add(_currentNode);
        }

        private void ProcessNeighbors()
        {
            foreach (var neighbor in _currentNode.GetNeighbors().Where(n => !_closedNodes.Contains((T) n))) ProcessNeighbor((T) neighbor);
        }

        private void ProcessNeighbor(T neighbor)
        {
            var neighborMeta = _nodeMetas.Get(neighbor);
            var currentMeta = _nodeMetas.Get(_currentNode);

            var fromCost = currentMeta.FromCost + _currentNode.RealCostTo(neighbor);
            var toCost = neighbor.EstimatedCostTo(Destination);
            var totalCost = fromCost * Thoroughness + toCost * (1 - Thoroughness);

            if (!_openNodes.Contains(neighbor))
            {
                _openNodes.Add(neighbor);
                neighborMeta.Parent = _currentNode;
                neighborMeta.ToCost = toCost;
                neighborMeta.FromCost = fromCost;
                neighborMeta.TotalCost = totalCost;
            }
            else if (fromCost < neighborMeta.FromCost)
            {
                neighborMeta.Parent = _currentNode;
                neighborMeta.FromCost = fromCost;
                neighborMeta.TotalCost = totalCost;
            }
        }

        private IList<T> GetPath()
        {
            if (State != SolverState.Success) return null;
            return _path ?? (_path = BuildPath());
        }

        private IList<T> BuildPath()
        {
            var path = new List<T> {Destination};
            var cNode = Destination;

            do
            {
                cNode = _nodeMetas.Get(cNode).Parent;
                path.Add(cNode);
            } while (!cNode.Equals(Origin));

            path.Reverse();
            return path;
        }

        private double CalcCost()
        {
            if (Path == null) return 0;

            var cost = 0d;
            var last = Path.First();

            foreach (var next in Path.Skip(1))
            {
                cost += last.RealCostTo(next);
                last = next;
            }

            return cost;
        }
    }
}
