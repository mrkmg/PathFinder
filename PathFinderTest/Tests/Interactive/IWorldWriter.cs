using System;
using PathFinder.Solvers;
using PathFinderTest.Map;

namespace PathFinderTest.Tests.Interactive
{
    public interface IWorldWriter
    {
        void WriteResult(int testNum, decimal thoroughness, double cost, int ticks);
        void DrawPosition(int x, int y);
        void DrawPosition(int x, int y, int testNumber);
        void DrawPosition(int x, int y, PositionType type);
        void DrawPosition(int x, int y, ConsoleColor background, ConsoleColor foreground);
        void DrawWorld();
    }

    public enum PositionType
    {
        Normal,
        Open,
        Closed,
        Current,
        End
    }
}
