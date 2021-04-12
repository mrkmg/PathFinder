using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using PathFinder.Solvers.Generic;

namespace PathFinderUnitTests
{
    public class GenericTests
    {
        private TestGraph _testGraph;
        private TestGraphNode _start;
        private TestGraphNode _end;
        
        [SetUp]
        public void Setup()
        {
            _testGraph = new TestGraph();
            _start = _testGraph.GetNode(0, 0);
            _end = _testGraph.GetNode(9, 9);
        }

        [Test]
        public void AStarWithGreedOneTestPath()
        {
            var solver = new AStar<TestGraphNode>(_start, _end);
            solver.Start();
            Assert.NotNull(solver.Path);
            DumpPath(solver.Path);
            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new List<TestGraphNode> {_testGraph.GetNode(1,1), _testGraph.GetNode(2,2), _testGraph.GetNode(1,3), _testGraph.GetNode(0,4), _testGraph.GetNode(1,5), _testGraph.GetNode(0,6), _testGraph.GetNode(0,7), _testGraph.GetNode(1,8), _testGraph.GetNode(2,9), _testGraph.GetNode(3,9), _testGraph.GetNode(4,9), _testGraph.GetNode(5,9), _testGraph.GetNode(6,9), _testGraph.GetNode(7,9), _testGraph.GetNode(8,9), _testGraph.GetNode(9,9)}, 
                    solver.Path
                );
                Assert.AreEqual(15.0d, solver.PathCost);
            });
        }
        
        [Test]
        public void AStarWithGreedZeroTestPath()
        {
            var solver = new AStar<TestGraphNode>(_start, _end, 0);
            solver.Start();
            Assert.NotNull(solver.Path);
            DumpPath(solver.Path);
            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new List<TestGraphNode> {_testGraph.GetNode(1,1), _testGraph.GetNode(2,2), _testGraph.GetNode(1,3), _testGraph.GetNode(0,4), _testGraph.GetNode(1,5), _testGraph.GetNode(0,6), _testGraph.GetNode(0,7), _testGraph.GetNode(1,8), _testGraph.GetNode(2,9), _testGraph.GetNode(3,9), _testGraph.GetNode(4,9), _testGraph.GetNode(5,9), _testGraph.GetNode(6,9), _testGraph.GetNode(7,9), _testGraph.GetNode(8,9), _testGraph.GetNode(9,9)},
                solver.Path
                );
                Assert.AreEqual(15.0d, solver.PathCost);
            });
        }
        
        [Test]
        public void AStarWithGreedTwoTestPath()
        {
            var solver = new AStar<TestGraphNode>(_start, _end, 8);
            solver.Start();
            Assert.NotNull(solver.Path);
            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new List<TestGraphNode> {_testGraph.GetNode(1,1), _testGraph.GetNode(2,2), _testGraph.GetNode(3,3), _testGraph.GetNode(4,3), _testGraph.GetNode(5,3), _testGraph.GetNode(6,4), _testGraph.GetNode(6,5), _testGraph.GetNode(7,6), _testGraph.GetNode(8,7), _testGraph.GetNode(9,8), _testGraph.GetNode(9,9)}, 
                    solver.Path
                );
                Assert.AreEqual(28.0d, solver.PathCost);
            });
        }

        [Test]
        public void BreadthFirstPath()
        {
            var solver = new BreadthFirst<TestGraphNode>(_start, _end);
            solver.Start();
            Assert.NotNull(solver.Path);
            DumpPath(solver.Path);
            Assert.Multiple(() =>
            {
                
                CollectionAssert.AreEqual(
                    new List<TestGraphNode> {_testGraph.GetNode(0,1), _testGraph.GetNode(0,2), _testGraph.GetNode(0,3), _testGraph.GetNode(0,4), _testGraph.GetNode(0,5), _testGraph.GetNode(0,6), _testGraph.GetNode(0,7), _testGraph.GetNode(1,8), _testGraph.GetNode(2,9), _testGraph.GetNode(3,8), _testGraph.GetNode(4,7), _testGraph.GetNode(5,6), _testGraph.GetNode(6,6), _testGraph.GetNode(7,7), _testGraph.GetNode(8,8), _testGraph.GetNode(9,9)}, 
                    solver.Path
                );
                Assert.AreEqual(15.0d, Math.Round(solver.PathCost));
            });
        }

        [Test]
        public void GreedyPath()
        {
            var solver = new Greedy<TestGraphNode>(_start, _end);
            solver.Start();
            Assert.NotNull(solver.Path);
            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new List<TestGraphNode> {_testGraph.GetNode(1,1), _testGraph.GetNode(2,2), _testGraph.GetNode(3,3), _testGraph.GetNode(4,3), _testGraph.GetNode(5,3), _testGraph.GetNode(6,4), _testGraph.GetNode(6,5), _testGraph.GetNode(7,6), _testGraph.GetNode(8,7), _testGraph.GetNode(9,8), _testGraph.GetNode(9,9)}, 
                    solver.Path
                );
                Assert.AreEqual(28.0d, Math.Round(solver.PathCost));
            });
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private static void DumpPath(IEnumerable<TestGraphNode> nodes)
        {
            TestContext.Write("new List<TestGraphNode> {");
            TestContext.Write(string.Join(", ", nodes.Select(n => $"_testGraph.GetNode({n.X},{n.Y})")));
            TestContext.Write("}");

        }
    }
}