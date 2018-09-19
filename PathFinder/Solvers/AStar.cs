using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Components;
using PathFinder.Interfaces;

namespace PathFinder.Solvers
{
    public class AStar<T> : ISolver<T> where T : INode
    {
        public static IList<T> Solve(T origin, T destination, double thoroughness = 0.5)
        {
            var aStar = new AStar<T>(origin, destination);
            while (aStar.State != SolverState.Running) aStar.Tick();
            return aStar.State == SolverState.Success ? aStar.Path : null;
        }

        private readonly HashSet<NodeMetaData<T>> _closedNodes = new HashSet<NodeMetaData<T>>();
        private readonly HashSet<NodeMetaData<T>> _openNodes = new HashSet<NodeMetaData<T>>();
        private readonly NodeMetas<T> _nodeMetas = new NodeMetas<T>();
        private IList<T> _path;
        private NodeMetaData<T> _current;
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
            _current = _nodeMetas.Get(origin);
            _openNodes.Add(_current);
        }

        /// <summary>
        ///     A List of nodes which still need to be checked
        /// </summary>
        public IEnumerable<T> Open => _openNodes.Select(n => n.Obj);

        /// <summary>
        ///     A list of nodes which have already been checked
        /// </summary>
        public IEnumerable<T> Closed => _closedNodes.Select(n => n.Obj);

        /// <summary>
        ///     The last node to be checked
        /// </summary>
        public T Current => _current.Obj;

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

            Ticks++;

            ProcessNeighbors();

            if (Current.Equals(Destination))
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
            return Math.Abs(lNode.TotalCost - tNode.TotalCost) < 0.01d ? lNode.ToCost < tNode.ToCost ? lNode : tNode :
                lNode.TotalCost < tNode.TotalCost ? lNode : tNode;
        }

        private void SetCurrentNode()
        {
            _current = _openNodes.Aggregate(LowestNodeAggregate);

            _openNodes.Remove(_current);
            _closedNodes.Add(_current);
        }

        private void ProcessNeighbors()
        {
            Current
                .GetNeighbors()
                .Select(neighbor => _nodeMetas.Get((T)neighbor))
                .Where(n => !_closedNodes.Contains(n))
                .ForAll(ProcessNeighbor);
        }

        private void ProcessNeighbor(NodeMetaData<T> neighbor)
        {
            var fromCost = _current.FromCost + Current.RealCostTo(neighbor.Obj);
            var toCost = neighbor.Obj.EstimatedCostTo(Destination);
            var totalCost = fromCost * Thoroughness + toCost * (1 - Thoroughness);

            if (!_openNodes.Contains(neighbor))
            {
                _openNodes.Add(neighbor);
                neighbor.Parent = Current;
                neighbor.ToCost = toCost;
                neighbor.FromCost = fromCost;
                neighbor.TotalCost = totalCost;
            }
            else if (Math.Abs(fromCost - neighbor.FromCost) < 0.01 && toCost < neighbor.ToCost || totalCost < neighbor.TotalCost)
            {
                neighbor.Parent = Current;
                neighbor.ToCost = toCost;
                neighbor.FromCost = fromCost;
                neighbor.TotalCost = totalCost;
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
