# Path Finder

A simple to use collection of various path finding algorithms.

This is a work in progress. All API's are subject to change **significantly**.

## Goals

The goal of this project is to provide a variety of easy to use and well
performing path finding algorithms that can be used with little modification
of a projects existing code.

### Why?

A* and other graph solvers are well documented, and honestly pretty "easy" to
implement right? Well as I found out, not exactly. Yes they are well
documented, but actually implementing them with good performance is another
story. Also, most implementations I found were very specific to the project
they were implemented in, and did not allow for much "configuration" of how
the solver would perform. This project was born from that frustration. 

## Installing

Currently Path Finder is not published to NuGet. Once it is in a more
stable state, it will be published.

To use in an existing project, you only need the `PathFinder` project, and the 
C5 library.

## Usage

This will change once the project is published to NuGet

1. Clone Path Finder to your machine.
2. Build `PathFinder`.
3. Add a reference in your existing project to `PathFinder.dll`.


## Future Plans

- Adding "Dynamic" algorithms which modify their running parameters based on 
  a set of configurable metrics (time to solve, distance remaining, etc).
- Adding "Spatially aware" path finding algorithms (Hierarchical, clustered, 
  etc).
- Adding Thread Safety to the solvers (or maybe thread safe alternatives at 
  the cost of performance).
- Testing in Unity and MonoGame.
- Publishing to NuGet.

## License

The MIT License (MIT)
Copyright © 2021 Kevin Gravier

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.