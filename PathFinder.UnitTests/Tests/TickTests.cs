using NUnit.Framework;
using PathFinder.Solvers.Generic;
using PathFinder.UnitTests.Fixtures;

namespace PathFinder.UnitTests.Tests
{
    public class TickTests
    {
        [Test]
        public void TestRunTicks()
        {
            var graph = new TestGraph();
            var solver = new Greedy<TestNode>(graph.GetNode(0, 0), graph.GetNode(9, 9));
            solver.Run(1);
            Assert.AreEqual(SolverState.Incomplete, solver.State);
        }

        [Test]
        public void TestMaxTicks()
        {
            var graph = new TestGraph();
            var solver = new Greedy<TestNode>(graph.GetNode(0, 0), graph.GetNode(9, 9))
            {
                MaxTicks = 5
            };
            solver.Run();
            Assert.AreEqual(SolverState.Failure, solver.State);
        }

        [Test]
        public void TestNoPath()
        {
            var graph = new TestGraph();
            var solver = new Greedy<TestNode>(graph.GetNode(0, 0), graph.GetNode(9, 0));
            solver.Run();
            Assert.AreEqual(SolverState.Failure, solver.State);
        }
    }
}