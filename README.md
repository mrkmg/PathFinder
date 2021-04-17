﻿# Path Finder

A simple to use collection of various path finding algorithms.

This is a work in progress. All API's are subject to change **significantly**.

## Goals

The goal of this project is to provide a variety of easy to use and well
performing path finding algorithms that can be used with little modification of
a projects existing code.

### Why?

A* and other graph solvers are well documented, and honestly pretty "easy" to
implement right? Well as I found out, not exactly. Yes they are well documented,
but actually implementing them with good performance is another story. Also,
most implementations I found were very specific to the project they were
implemented in, and did not allow for much "
configuration" of how the solver would perform. This project was born from that
frustration.

## Installing

Currently Path Finder is not published to NuGet. Once it is in a more stable
state, it will be published.

To test it out in its current state, build and reference PathFinder.dll, and
install C5 from NuGet.

## Usage

PathFinder uses the term "Graph" to represent any network of connected nodes. A
2d or 3d map is also a graph, with each position on the map being nodes in the
graph connected by traversing through the map. A Graph could represent anything
though, for example persons in a social network, with friendships allowing
traversal through the graph.

### Simple Usage

First, update your current "nodes" to
implement [`ITraversableGraphNode`](PathFinder/Graphs/ITraversableGraphNode.cs).
When implementing, it is important to make sure `EstimatedCostTo` returns the
**best case** cost for the best performance. A decent example can be found
in [SimpleWorld.Map.Position](Extras/SimpleWorld/Map/Position.cs).

To find a path, use one of the included solvers.

```c#
using PathFinder.Solvers.Generic;

YourNode fromNode = YourGraph.GetNode();
YourNode toNode = YourGraph.GetNode();

// just get a path using A*
var didGetPath = AStar.Solve(fromNode, toNode, out IList path);

// create a solver and run it manually
var solver = new Greedy(fromNode, toNode);
solver.MaxTicks = 20000;
while (solver.State == SolverState.Waiting) {
    solver.Start(100); // run 100 ticks
    DoOtherWork();
}
if (solver.State == SolverState.Success)
    MoveEntity(entity, solver.Path)
```

### Solver Types

All Generic solvers
implement [IGraphSolver](PathFinder/Solvers/Generic/IGraphSolver.cs).

[Solvers.Generic.AStar](PathFinder/Solvers/Generic/AStar.cs)

An implementation of the traditional A* algorithm with an adjustable greed
factor.

The greed factor determines how exhaustive the search will be. Greed factors
closer to zero will check more possible nodes, while greater than zero will
favor reducing the estimated. A greed factor of one is equivalent to the
standard A*
method.

[Solvers.Generic.Greedy](PathFinder/Solvers/Generic/Greedy.cs)

The greedy solvers traverses a graph ignoring any travel costs and attempts to
reduce the remaining distance only. This can be useful to determine if a path is
even possible, or in very open graphs with little to no obstacles or movement
costs.

[Solvers.Generic.BreadthFirst](PathFinder/Solvers/Generic/BreadthFirst.cs)

An exhaustive graph searcher, or crawler. Starts at node and searches every
found node until the destination is found. This is really only useful in graphs
when an estimation function is impossible, as the A* algorithm with greed <= 1
will always find the best path, at a fraction of the cost of Breadth First.

### Node Traversers

All of the generic solvers can be given
an [INodeTraverser](PathFinder/Graphs/INodeTraverser.cs)
to replace or augment the graphs own traversal methods. An example of when to
use an INodeTraverser could be to implement custom traversal patterns in a game
on the entities themselves.

## Future Plans

- Writing more documentation/guides to using.
- Adding "Dynamic" algorithms which modify their running parameters based on a
  set of configurable metrics (time to solve, distance remaining, etc).
- Adding "Spatially aware" path finding algorithms (Hierarchical, clustered, ray
  tracing, etc).
- Adding Thread Safety to the solvers (or maybe thread safe alternatives at the
  cost of performance).
- Testing in Unity and MonoGame.
- Publishing to NuGet.

## License

The MIT License (MIT)
Copyright © 2021 Kevin Gravier

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the “Software”), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.