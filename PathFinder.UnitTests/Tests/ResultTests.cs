using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using PathFinder.Solvers.Generic;
using PathFinder.UnitTests.Fixtures;

namespace PathFinder.UnitTests.Tests
{
    public class ResultTests
    {
        private TestGraph _testGraph;
        private TestNode _start;
        private TestNode _end;
        
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
            var solver = new AStar<TestNode>(_start, _end);
            solver.Run();
            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(0,1), _testGraph.GetNode(0,2), _testGraph.GetNode(0,3), _testGraph.GetNode(0,4), _testGraph.GetNode(0,5), _testGraph.GetNode(0,6), _testGraph.GetNode(0,7), _testGraph.GetNode(1,8), _testGraph.GetNode(2,9), _testGraph.GetNode(3,9), _testGraph.GetNode(4,9), _testGraph.GetNode(5,9), _testGraph.GetNode(6,9), _testGraph.GetNode(7,9), _testGraph.GetNode(8,9), _testGraph.GetNode(9,9)}, 
                    solver.Path
                );
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(3,8), _testGraph.GetNode(3,5), _testGraph.GetNode(5,8), _testGraph.GetNode(2,5), _testGraph.GetNode(4,8), _testGraph.GetNode(0,9), _testGraph.GetNode(6,8), _testGraph.GetNode(3,7), _testGraph.GetNode(9,8), _testGraph.GetNode(3,6), _testGraph.GetNode(1,9), _testGraph.GetNode(2,7), _testGraph.GetNode(7,0), _testGraph.GetNode(2,8), _testGraph.GetNode(6,4), _testGraph.GetNode(0,8), _testGraph.GetNode(7,8), _testGraph.GetNode(8,8), _testGraph.GetNode(6,5)}, 
                    solver.Open
                );
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(0,1), _testGraph.GetNode(1,0), _testGraph.GetNode(1,1), _testGraph.GetNode(0,0), _testGraph.GetNode(0,2), _testGraph.GetNode(1,2), _testGraph.GetNode(2,0), _testGraph.GetNode(2,1), _testGraph.GetNode(2,2), _testGraph.GetNode(1,3), _testGraph.GetNode(2,3), _testGraph.GetNode(3,1), _testGraph.GetNode(3,2), _testGraph.GetNode(3,3), _testGraph.GetNode(4,2), _testGraph.GetNode(4,3), _testGraph.GetNode(5,2), _testGraph.GetNode(5,3), _testGraph.GetNode(6,2), _testGraph.GetNode(6,3), _testGraph.GetNode(4,1), _testGraph.GetNode(5,1), _testGraph.GetNode(3,0), _testGraph.GetNode(0,3), _testGraph.GetNode(7,2), _testGraph.GetNode(7,3), _testGraph.GetNode(7,4), _testGraph.GetNode(8,3), _testGraph.GetNode(8,4), _testGraph.GetNode(4,0), _testGraph.GetNode(0,4), _testGraph.GetNode(6,1), _testGraph.GetNode(9,3), _testGraph.GetNode(9,4), _testGraph.GetNode(8,2), _testGraph.GetNode(5,0), _testGraph.GetNode(7,1), _testGraph.GetNode(6,0), _testGraph.GetNode(9,2), _testGraph.GetNode(0,5), _testGraph.GetNode(1,5), _testGraph.GetNode(0,6), _testGraph.GetNode(1,6), _testGraph.GetNode(2,6), _testGraph.GetNode(0,7), _testGraph.GetNode(1,7), _testGraph.GetNode(1,8), _testGraph.GetNode(2,9), _testGraph.GetNode(3,9), _testGraph.GetNode(4,9), _testGraph.GetNode(5,9), _testGraph.GetNode(6,9), _testGraph.GetNode(7,9), _testGraph.GetNode(8,9), _testGraph.GetNode(9,9)},
                    solver.Closed
                );
                Assert.AreEqual(16.0d, solver.PathCost);
            });
        }
        
        [Test]
        public void AStarWithGreedZeroTestPath()
        {
            var solver = new AStar<TestNode>(_start, _end, 0);
            solver.Run();
            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(0,1), _testGraph.GetNode(0,2), _testGraph.GetNode(0,3), _testGraph.GetNode(0,4), _testGraph.GetNode(0,5), _testGraph.GetNode(0,6), _testGraph.GetNode(0,7), _testGraph.GetNode(1,8), _testGraph.GetNode(2,9), _testGraph.GetNode(3,8), _testGraph.GetNode(4,7), _testGraph.GetNode(5,6), _testGraph.GetNode(6,6), _testGraph.GetNode(7,7), _testGraph.GetNode(8,8), _testGraph.GetNode(9,9)},
                    solver.Path
                );
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(9,8), _testGraph.GetNode(9,6), _testGraph.GetNode(9,7)}, 
                    solver.Open
                );
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(0,1), _testGraph.GetNode(1,0), _testGraph.GetNode(1,1), _testGraph.GetNode(0,0), _testGraph.GetNode(0,2), _testGraph.GetNode(1,2), _testGraph.GetNode(2,0), _testGraph.GetNode(2,1), _testGraph.GetNode(2,2), _testGraph.GetNode(1,3), _testGraph.GetNode(2,3), _testGraph.GetNode(3,1), _testGraph.GetNode(3,2), _testGraph.GetNode(3,3), _testGraph.GetNode(3,0), _testGraph.GetNode(0,3), _testGraph.GetNode(4,2), _testGraph.GetNode(4,3), _testGraph.GetNode(4,1), _testGraph.GetNode(4,0), _testGraph.GetNode(0,4), _testGraph.GetNode(5,2), _testGraph.GetNode(5,3), _testGraph.GetNode(5,1), _testGraph.GetNode(5,0), _testGraph.GetNode(0,5), _testGraph.GetNode(1,5), _testGraph.GetNode(6,2), _testGraph.GetNode(6,3), _testGraph.GetNode(6,4), _testGraph.GetNode(6,1), _testGraph.GetNode(0,6), _testGraph.GetNode(1,6), _testGraph.GetNode(2,5), _testGraph.GetNode(2,6), _testGraph.GetNode(6,0), _testGraph.GetNode(7,2), _testGraph.GetNode(7,3), _testGraph.GetNode(7,4), _testGraph.GetNode(7,1), _testGraph.GetNode(7,0), _testGraph.GetNode(0,7), _testGraph.GetNode(1,7), _testGraph.GetNode(2,7), _testGraph.GetNode(6,5), _testGraph.GetNode(8,3), _testGraph.GetNode(8,4), _testGraph.GetNode(8,2), _testGraph.GetNode(0,8), _testGraph.GetNode(1,8), _testGraph.GetNode(2,8), _testGraph.GetNode(9,3), _testGraph.GetNode(9,4), _testGraph.GetNode(9,2), _testGraph.GetNode(3,5), _testGraph.GetNode(3,6), _testGraph.GetNode(3,7), _testGraph.GetNode(0,9), _testGraph.GetNode(1,9), _testGraph.GetNode(2,9), _testGraph.GetNode(3,8), _testGraph.GetNode(3,9), _testGraph.GetNode(4,8), _testGraph.GetNode(4,9), _testGraph.GetNode(4,7), _testGraph.GetNode(5,8), _testGraph.GetNode(5,9), _testGraph.GetNode(5,7), _testGraph.GetNode(4,6), _testGraph.GetNode(5,6), _testGraph.GetNode(6,8), _testGraph.GetNode(6,9), _testGraph.GetNode(6,7), _testGraph.GetNode(6,6), _testGraph.GetNode(7,6), _testGraph.GetNode(7,8), _testGraph.GetNode(7,9), _testGraph.GetNode(7,7), _testGraph.GetNode(8,8), _testGraph.GetNode(8,9), _testGraph.GetNode(8,7), _testGraph.GetNode(8,6), _testGraph.GetNode(9,9)},
                    solver.Closed
                );
                Assert.AreEqual(16.0d, solver.PathCost);
            });
        }
        
        [Test]
        public void AStarWithGreedTwoTestPath()
        {
            var solver = new AStar<TestNode>(_start, _end, 8);
            solver.Run();
            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(1,1), _testGraph.GetNode(2,2), _testGraph.GetNode(3,3), _testGraph.GetNode(4,3), _testGraph.GetNode(5,3), _testGraph.GetNode(6,4), _testGraph.GetNode(6,5), _testGraph.GetNode(7,6), _testGraph.GetNode(8,7), _testGraph.GetNode(9,8), _testGraph.GetNode(9,9)}, 
                    solver.Path
                );
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(8,9), _testGraph.GetNode(0,0), _testGraph.GetNode(8,6), _testGraph.GetNode(0,1), _testGraph.GetNode(8,8), _testGraph.GetNode(0,2), _testGraph.GetNode(7,2), _testGraph.GetNode(1,0), _testGraph.GetNode(6,7), _testGraph.GetNode(2,1), _testGraph.GetNode(7,8), _testGraph.GetNode(2,0), _testGraph.GetNode(9,7), _testGraph.GetNode(3,2), _testGraph.GetNode(3,1), _testGraph.GetNode(1,2), _testGraph.GetNode(5,6), _testGraph.GetNode(6,2), _testGraph.GetNode(9,2), _testGraph.GetNode(1,3), _testGraph.GetNode(6,6), _testGraph.GetNode(8,2), _testGraph.GetNode(5,2), _testGraph.GetNode(2,3), _testGraph.GetNode(7,7), _testGraph.GetNode(9,6), _testGraph.GetNode(4,2)}, 
                    solver.Open
                );
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(1,1), _testGraph.GetNode(2,2), _testGraph.GetNode(3,3), _testGraph.GetNode(4,3), _testGraph.GetNode(5,3), _testGraph.GetNode(6,3), _testGraph.GetNode(6,4), _testGraph.GetNode(6,5), _testGraph.GetNode(7,3), _testGraph.GetNode(7,4), _testGraph.GetNode(8,3), _testGraph.GetNode(8,4), _testGraph.GetNode(9,3), _testGraph.GetNode(9,4), _testGraph.GetNode(7,6), _testGraph.GetNode(8,7), _testGraph.GetNode(9,8), _testGraph.GetNode(9,9)},
                    solver.Closed
                );
                Assert.AreEqual(29.0d, solver.PathCost);
            });
        }

        [Test]
        public void BreadthFirstPath()
        {
            var solver = new BreadthFirst<TestNode>(_start, _end);
            solver.Run();
            DumpPath(solver.Open);
            DumpPath(solver.Closed);
            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(0,1), _testGraph.GetNode(0,2), _testGraph.GetNode(0,3), _testGraph.GetNode(0,4), _testGraph.GetNode(0,5), _testGraph.GetNode(0,6), _testGraph.GetNode(0,7), _testGraph.GetNode(1,8), _testGraph.GetNode(2,9), _testGraph.GetNode(3,8), _testGraph.GetNode(4,7), _testGraph.GetNode(5,6), _testGraph.GetNode(6,6), _testGraph.GetNode(7,7), _testGraph.GetNode(8,8), _testGraph.GetNode(9,9)}, 
                    solver.Path
                );
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(7,6)}, 
                    solver.Open
                );
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(0,1), _testGraph.GetNode(1,0), _testGraph.GetNode(1,1), _testGraph.GetNode(0,0), _testGraph.GetNode(0,2), _testGraph.GetNode(1,2), _testGraph.GetNode(2,0), _testGraph.GetNode(2,1), _testGraph.GetNode(2,2), _testGraph.GetNode(0,3), _testGraph.GetNode(1,3), _testGraph.GetNode(2,3), _testGraph.GetNode(3,0), _testGraph.GetNode(3,1), _testGraph.GetNode(3,2), _testGraph.GetNode(3,3), _testGraph.GetNode(0,4), _testGraph.GetNode(4,0), _testGraph.GetNode(4,1), _testGraph.GetNode(4,2), _testGraph.GetNode(4,3), _testGraph.GetNode(0,5), _testGraph.GetNode(1,5), _testGraph.GetNode(5,0), _testGraph.GetNode(5,1), _testGraph.GetNode(5,2), _testGraph.GetNode(5,3), _testGraph.GetNode(0,6), _testGraph.GetNode(1,6), _testGraph.GetNode(2,5), _testGraph.GetNode(2,6), _testGraph.GetNode(6,0), _testGraph.GetNode(6,1), _testGraph.GetNode(6,2), _testGraph.GetNode(6,3), _testGraph.GetNode(6,4), _testGraph.GetNode(0,7), _testGraph.GetNode(1,7), _testGraph.GetNode(7,0), _testGraph.GetNode(7,1), _testGraph.GetNode(7,2), _testGraph.GetNode(7,3), _testGraph.GetNode(7,4), _testGraph.GetNode(2,7), _testGraph.GetNode(0,8), _testGraph.GetNode(1,8), _testGraph.GetNode(8,2), _testGraph.GetNode(8,3), _testGraph.GetNode(8,4), _testGraph.GetNode(6,5), _testGraph.GetNode(2,8), _testGraph.GetNode(3,5), _testGraph.GetNode(3,6), _testGraph.GetNode(3,7), _testGraph.GetNode(0,9), _testGraph.GetNode(1,9), _testGraph.GetNode(2,9), _testGraph.GetNode(9,2), _testGraph.GetNode(9,3), _testGraph.GetNode(9,4), _testGraph.GetNode(3,8), _testGraph.GetNode(3,9), _testGraph.GetNode(4,7), _testGraph.GetNode(4,8), _testGraph.GetNode(4,9), _testGraph.GetNode(4,6), _testGraph.GetNode(5,6), _testGraph.GetNode(5,7), _testGraph.GetNode(5,8), _testGraph.GetNode(5,9), _testGraph.GetNode(6,6), _testGraph.GetNode(6,7), _testGraph.GetNode(6,8), _testGraph.GetNode(6,9), _testGraph.GetNode(7,7), _testGraph.GetNode(7,8), _testGraph.GetNode(7,9), _testGraph.GetNode(8,6), _testGraph.GetNode(8,7), _testGraph.GetNode(8,8), _testGraph.GetNode(8,9), _testGraph.GetNode(9,6), _testGraph.GetNode(9,7), _testGraph.GetNode(9,8), _testGraph.GetNode(9,9)},
                    solver.Closed
                );
                Assert.AreEqual(16.0d, Math.Round(solver.PathCost));
            });
        }

        [Test]
        public void GreedyPath()
        {
            var solver = new Greedy<TestNode>(_start, _end);
            solver.Run();
            DumpPath(solver.Open);
            DumpPath(solver.Closed);
            Assert.Multiple(() =>
            {
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(1,1), _testGraph.GetNode(2,2), _testGraph.GetNode(3,3), _testGraph.GetNode(4,3), _testGraph.GetNode(5,3), _testGraph.GetNode(6,4), _testGraph.GetNode(6,5), _testGraph.GetNode(7,6), _testGraph.GetNode(8,7), _testGraph.GetNode(9,8), _testGraph.GetNode(9,9)}, 
                    solver.Path
                );
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(8,9), _testGraph.GetNode(0,0), _testGraph.GetNode(8,6), _testGraph.GetNode(1,0), _testGraph.GetNode(8,8), _testGraph.GetNode(2,0), _testGraph.GetNode(7,3), _testGraph.GetNode(0,1), _testGraph.GetNode(6,7), _testGraph.GetNode(1,2), _testGraph.GetNode(7,8), _testGraph.GetNode(0,2), _testGraph.GetNode(9,7), _testGraph.GetNode(2,3), _testGraph.GetNode(1,3), _testGraph.GetNode(2,1), _testGraph.GetNode(6,3), _testGraph.GetNode(6,2), _testGraph.GetNode(7,4), _testGraph.GetNode(3,1), _testGraph.GetNode(6,6), _testGraph.GetNode(5,6), _testGraph.GetNode(5,2), _testGraph.GetNode(3,2), _testGraph.GetNode(7,7), _testGraph.GetNode(9,6), _testGraph.GetNode(4,2)}, 
                    solver.Open
                );
                CollectionAssert.AreEqual(
                    new List<TestNode> {_testGraph.GetNode(1,1), _testGraph.GetNode(2,2), _testGraph.GetNode(3,3), _testGraph.GetNode(4,3), _testGraph.GetNode(5,3), _testGraph.GetNode(6,4), _testGraph.GetNode(6,5), _testGraph.GetNode(7,6), _testGraph.GetNode(8,7), _testGraph.GetNode(9,8), _testGraph.GetNode(9,9)},
                    solver.Closed
                );
                Assert.AreEqual(29.0d, Math.Round(solver.PathCost));
            });
        }

        /// <summary>
        /// Used to assist in creating tests. Outputs to the test runner the path that can be pasted into CollectionAssert.
        /// </summary>
        /// <param name="nodes"></param>
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private static void DumpPath(IEnumerable<TestNode> nodes)
        {
            TestContext.Write("new List<TestNode> {");
            TestContext.Write(string.Join(", ", nodes.Select(n => $"_testGraph.GetNode({n.X},{n.Y})")));
            TestContext.Write("}");
            TestContext.WriteLine();
        }
    }
}