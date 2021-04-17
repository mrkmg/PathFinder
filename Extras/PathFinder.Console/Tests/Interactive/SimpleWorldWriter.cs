using System;
using System.Globalization;
using SimpleWorld.Map;

namespace PathFinder.Console.Tests.Interactive
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
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.CursorLeft = 0;
            System.Console.CursorTop = 0;
            System.Console.Write(info);
        }

        public void WriteResult(int testNum, double thoroughness, double cost, int ticks, long cpuCycles)
        {
            System.Console.BackgroundColor = ConsoleColor.White;
            System.Console.ForegroundColor = ConsoleColor.Black;
            System.Console.CursorLeft = 0;
            System.Console.CursorTop = System.Console.WindowHeight - testNum - 1;
            System.Console.Write(testNum.ToString().PadRight(5) + " | " +
                                 thoroughness.ToString(CultureInfo.CurrentCulture).PadRight(5) + " | " +
                                 Math.Ceiling(cost).ToString(CultureInfo.CurrentCulture).PadLeft(6) + " / " +
                                 ticks.ToString().PadLeft(6) + " / " +
                                 cpuCycles.ToString().PadLeft(12));
        }

        public void DrawSeed(int seed)
        {
            WriteInfo($"Seed: {seed}");
        }

        public void DrawPosition(int x, int y)
        {
            var node = _world.GetPosition(x, y);
            if (node != null)
            {
                DrawPosition(x, y, GetWorldBackground(node.Cost), GetWorldForeground(node.Cost));
            }
            else
                DrawPosition(x, y, ConsoleColor.Black, ConsoleColor.White);
        }

        public void DrawPosition(int x, int y, int testNumber)
        {
            DrawPosition(x, y, ConsoleColor.Green, ConsoleColor.Black);
        }

        public void DrawPosition(int x, int y, PositionType type)
        {
            switch (type)
            {
                case PositionType.Normal:
                    DrawPosition(x, y);
                    break;
                case PositionType.Open:
                    DrawPosition(x, y, ConsoleColor.Yellow, ConsoleColor.Black);
                    break;
                case PositionType.Closed:
                    DrawPosition(x, y, ConsoleColor.Green, ConsoleColor.Black);
                    break;
                case PositionType.Current:
                    DrawPosition(x, y, ConsoleColor.Black, ConsoleColor.White);
                    break;
                case PositionType.End:
                    DrawPosition(x, y, ConsoleColor.White, ConsoleColor.Black);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void DrawPosition(int x, int y, ConsoleColor background, ConsoleColor foreground)
        {
            if (x < ResultWidth && y > System.Console.WindowHeight - _totalTests - 1) return;

            System.Console.CursorLeft = x;
            System.Console.CursorTop = y;
            System.Console.BackgroundColor = background;
            System.Console.ForegroundColor = foreground;
            var node = _world.GetPosition(x, y);
            if (node != null)
                System.Console.Write(node.Cost);
            else
                System.Console.Write(" ");

            System.Console.ResetColor();
        }

        public void DrawWorld()
        {
            for (var y = 0; y < _world.YSize; y++)
            for (var x = 0; x < _world.XSize; x++)
                DrawPosition(x, y);
        }

        public ConsoleColor GetWorldBackground(int level)
        {
            switch (level)
            {
                case 1: return ConsoleColor.Blue;
                case 2: return ConsoleColor.Cyan;
                case 3: return ConsoleColor.Magenta;
                case 4: return ConsoleColor.Red;
                case 9: return ConsoleColor.Black;
                default: return ConsoleColor.Gray;
            }
        }
        
        public ConsoleColor GetWorldForeground(int level)
        {
            return level == 9 ? ConsoleColor.White : ConsoleColor.Black;
        }
    }
}
