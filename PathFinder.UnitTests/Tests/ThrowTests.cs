using System;
using System.Collections.Generic;
using NUnit.Framework;
using PathFinder.Graphs;
using PathFinder.Solvers.Generic;
using PathFinder.UnitTests.Fixtures;

namespace PathFinder.UnitTests.Tests
{
    public class ThrowTests
    {
        [Test]
        public void ThrowsOnNullArguments() =>
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => AStar.Solve(new TestingNode(), null, out _));
                Assert.Throws<ArgumentNullException>(() => AStar.Solve(null, new TestingNode(), out _));
            });

        [Test]
        public void ThrowsOnNoTraverserAndNotTraversable() => 
            Assert.Throws<ArgumentException>(() => AStar.Solve(new TestingNode(), new TestingNode(), out _));

        [Test]
        public void ThrowsOnNodeWhichReturnsNullNeighbor() =>
            Assert.Throws<ArgumentNullException>(() =>
                AStar.Solve(new NodeWhichReturnsNullNeighbors(), new NodeWhichReturnsNullNeighbors(), out _));

        [Test]
        public void ThrowsWhenTryingToRunCompletedSolver()
        {
            var testGraph = new TestGraph();
            var solver = new Greedy<TestNode>(testGraph.GetNode(0, 0), testGraph.GetNode(9, 9));
            solver.Run();
            Assert.Throws<InvalidOperationException>(() => solver.Run());
        }
    }

    internal class TestingNode : IEquatable<TestingNode>
    {
        public bool Equals(TestingNode other) => throw new NotImplementedException();
        public override bool Equals(object obj) => throw new NotImplementedException();
        public override int GetHashCode() => throw new NotImplementedException();
    }

    #nullable enable
    internal class NodeWhichReturnsNullNeighbors : ITraversableNode<NodeWhichReturnsNullNeighbors>
    {
        public bool Equals(NodeWhichReturnsNullNeighbors? other) => false;

        public double RealCostTo(NodeWhichReturnsNullNeighbors other) => 1;

        public double EstimatedCostTo(NodeWhichReturnsNullNeighbors other) => 1;

        public IEnumerable<NodeWhichReturnsNullNeighbors> NeighborNodes()
        {
            yield break;
        }
    }
    #nullable disable
}