using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Components;
using PathFinder.Interfaces;

namespace PathFinder.Solvers
{
    public class AStar<T> : Solver<T> where T : INode
    {
        /// <summary>
        /// A List of nodes which still need to be checked
        /// </summary>
        public IEnumerable<T> Open => _openNodes.Select(n => n.Obj);

        /// <summary>
        /// A list of nodes which have already been checked
        /// </summary>
        public IEnumerable<T> Closed => _closedNodes.Select(n => n.Obj);

        /// <summary>
        /// The last node to be checked
        /// </summary>
        public T Current => _currentNode.Obj;

        /// <summary>
        /// How thorough the search should be. Should be a value between 0 and 1.
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

        public double Cost => DestinationNode.FromCost;

        private readonly HashSet<Node<T>> _openNodes = new HashSet<Node<T>>();
        private readonly HashSet<Node<T>> _closedNodes = new HashSet<Node<T>>();
        private double _thoroughness = 0.5;
        private Node<T> _currentNode;

        /// <summary>
        /// Creates a solver using the A* method
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="destination">The destination</param>
        public AStar(T origin, T destination) : base(origin, destination)
        {
            _openNodes.Add(OriginNode);
        }

        /// <summary>
        /// Check one node
        /// </summary>
        public override void Tick()
        {
            Ticks++;

            if (State != SolverState.Running)
            {
                throw new Exception("Not running");
            }

            if (_openNodes.Count == 0)
            {
                State = SolverState.Failed;
                return;
            }

            SetCurrentNode();

            if (_currentNode == DestinationNode)
            {
                State = SolverState.Success;
            }
            else
            {
                ProcessNeighbors();
            }
        }

        private void SetCurrentNode()
        {
            _currentNode = _openNodes.Aggregate(LowestNodeAggregateAlt);

            _openNodes.Remove(_currentNode);
            _closedNodes.Add(_currentNode);
        }

        private static Node<T> LowestNodeAggregateAlt(Node<T> lNode, Node<T> tNode) =>
            tNode.TotalCost > lNode.TotalCost ? lNode : tNode;

        private static Node<T> LowestNodeAggregate(Node<T> lNode, Node<T> tNode) => tNode.TotalCost > lNode.TotalCost ||
                                                                                    Math.Abs(tNode.TotalCost - lNode.TotalCost) < 1 && 
                                                                                    tNode.ToCost > lNode.ToCost ? lNode : tNode;

        private void ProcessNeighbors()
        {
            foreach (var neighbor in _currentNode.Neighbors().Select(GetNode).Where(n => !_closedNodes.Contains(n)))
            {
                ProcessNeighbor(neighbor);
            }
        }

        private void ProcessNeighbor(Node<T> neighbor)
        {
            var fromCost = _currentNode.FromCost + _currentNode.RealCostTo(neighbor);
            var toCost = neighbor.ToCost;
            var totalCost = fromCost * Thoroughness + toCost * (1 - Thoroughness);

            if (!_openNodes.Contains(neighbor))
            {
                _openNodes.Add(neighbor);
                neighbor.Parent = _currentNode;
                neighbor.FromCost = fromCost;
                neighbor.TotalCost = totalCost;

            } else if (totalCost < neighbor.TotalCost)
            {
                neighbor.Parent = _currentNode;
                neighbor.FromCost = fromCost;
                neighbor.TotalCost = totalCost;
            }
        }

        protected override IList<T> BuildPath()
        {
            var path = new List<T> { DestinationNode.Obj };
            var cNode = DestinationNode;

            do
            {
                cNode = cNode.Parent;
                path.Add(cNode.Obj);
            } while (!cNode.Equals(OriginNode));

            path.Reverse();
            return path;
        }
    }
}
