using System.Drawing;

namespace SimpleWorld.MazeGenerator
{
    internal static class RoomTemplates
    {
        
        public static RoomTemplate Square = new (
            new Point[] { new (-2, -2), new (2, -2), new (2, 2), new (-2, 2) },
            new Point[] { new (0, -2), new (-2, 0), new (0, 2), new (2, 0)}   
        );
        
        public static RoomTemplate H = new (
            new Point[] { 
                new (-4, -4), 
                new (-4, 4), 
                new (-3, 4), 
                new (-3, 1), 
                new (3, 1), 
                new (3, 4), 
                new (4, 4), 
                new (4, -4), 
                new (3, -4), 
                new (3, -1),
                new (-3, -1), 
                new (-3, -4) },
            new Point[] { new (0, -1), new (0, 1) }
        );

        public static RoomTemplate L = new (
            new Point[] { new (-2, -4), new (-2, 4), new (2, 4), new (2, 2), new (0, 2), new (0, -4) },
            new Point[] { new (1, 2) }
        );

        public static RoomTemplate U = new (
            new Point[] { 
                new (-4, -4), 
                new (-4, 4), 
                new (4, 4), 
                new (4, -4), 
                new (2, -4), 
                new (2, 2), 
                new (-2, 2), 
                new (-2, -4) },
            new Point[] { new (-3, -4), new (3, 4) }
        );
        
        public static RoomTemplate Plus = new (
            new Point[] { 
                new (-1, -4), 
                new (-1, -1), 
                new (-4, -1), 
                new (-4, 1), 
                new (-1, 1), 
                new (-1, 4), 
                new (1, 4), 
                new (1, 1), 
                new (4, 1), 
                new (4, -1), 
                new (1, -1), 
                new (1, -4) },
            new Point[] { new (0, 4), new (0, -4), new (4, 0), new (-4, 0) }
        );
        
        public static readonly RoomTemplate[] AllTemplates =
        {
            Square, Plus, H, L, U
        };
    }
}