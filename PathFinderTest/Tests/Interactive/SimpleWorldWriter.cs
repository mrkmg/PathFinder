using System;
using System.Globalization;
using PathFinderTest.Map;

namespace PathFinderTest.Tests.Interactive
{
    public class SimpleWorldWriter : IWorldWriter
    {
        private const int ResultWidth = 46;
        private readonly int _totalTests;
        private readonly World _world;

        public SimpleWorldWriter(World world, int totalTests)
        {
            _world = world;
            _totalTests = totalTests;
        }

        public void WriteInfo(string info)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorLeft = 0;
            Console.CursorTop = 0;
            Console.Write(info);
        }

        public void WriteResult(int testNum, decimal thoroughness, double cost, int ticks, long cpuCycles)
        {
            Console.BackgroundColor = GetBackgroundColor(testNum);
            Console.ForegroundColor = GetForegroundColor(testNum);
            Console.CursorLeft = 0;
            Console.CursorTop = Console.WindowHeight - testNum - 1;
            Console.Write(testNum.ToString().PadRight(5) + " | " +
                          thoroughness.ToString(CultureInfo.CurrentCulture).PadRight(5) + " | " +
                          Math.Ceiling(cost).ToString(CultureInfo.CurrentCulture).PadLeft(6) + " / " +
                          ticks.ToString().PadLeft(6) + " / " +
                          cpuCycles.ToString().PadLeft(12));
        }

        public void DrawPosition(int x, int y)
        {
            if (_world.AllNodes[x].ContainsKey(y))
                DrawPosition(x, y, ConsoleColor.White, ConsoleColor.Black);
            else
                DrawPosition(x, y, ConsoleColor.Black, ConsoleColor.White);
        }

        public void DrawPosition(int x, int y, int testNumber)
        {
            DrawPosition(x, y, GetBackgroundColor(testNumber), GetForegroundColor(testNumber));
        }

        public void DrawPosition(int x, int y, PositionType type)
        {
            switch (type)
            {
                case PositionType.Normal:
                    DrawPosition(x, y);
                    break;
                case PositionType.Open:
                    DrawPosition(x, y, ConsoleColor.Red, ConsoleColor.Black);
                    break;
                case PositionType.Closed:
                    DrawPosition(x, y, ConsoleColor.Green, ConsoleColor.Black);
                    break;
                case PositionType.Current:
                    DrawPosition(x, y, ConsoleColor.Black, ConsoleColor.White);
                    break;
                case PositionType.End:
                    DrawPosition(x, y, ConsoleColor.Blue, ConsoleColor.White);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void DrawPosition(int x, int y, ConsoleColor background, ConsoleColor foreground)
        {
            if (x < ResultWidth && y > Console.WindowHeight - _totalTests - 1) return;

            Console.CursorLeft = x;
            Console.CursorTop = y;
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
            if (_world.AllNodes[x].ContainsKey(y))
                Console.Write(_world.AllNodes[x][y].Z);
            else
                Console.Write(" ");

            Console.ResetColor();
        }

        public void DrawWorld()
        {
            for (var y = 0; y < _world.YSize; y++)
            for (var x = 0; x < _world.XSize; x++)
                DrawPosition(x, y);
        }

        private static ConsoleColor GetBackgroundColor(int testNumber)
        {
            switch (testNumber % 5)
            {
                case 0: return ConsoleColor.Cyan;
                case 1: return ConsoleColor.Blue;
                case 2: return ConsoleColor.Green;
                case 3: return ConsoleColor.Yellow;
                default: return ConsoleColor.Magenta;
            }
        }

        private static ConsoleColor GetForegroundColor(int testNumber)
        {
            switch (testNumber % 5)
            {
                case 0: return ConsoleColor.Black;
                case 1: return ConsoleColor.White;
                case 2: return ConsoleColor.Black;
                case 3: return ConsoleColor.Black;
                default: return ConsoleColor.Black;
            }
        }
    }
}
