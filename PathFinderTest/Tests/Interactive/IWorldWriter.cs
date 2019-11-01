using System;

namespace PathFinderTest.Tests.Interactive
{
    public interface IWorldWriter
    {
        void WriteResult(int testNum, double thoroughness, double cost, int ticks, long cpuCycles);
        void WriteInfo(string info);
        void DrawPosition(int x, int y);
        void DrawPosition(int x, int y, int testNumber);
        void DrawPosition(int x, int y, PositionType type);
        void DrawPosition(int x, int y, ConsoleColor background, ConsoleColor foreground);
        void DrawSeed(int seed);
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
