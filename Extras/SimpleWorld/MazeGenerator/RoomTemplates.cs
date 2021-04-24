using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleWorld.MazeGenerator
{
    using XYList = List<(int X, int Y)>;
    using XYIEnumerable = IEnumerable<(int X, int Y)>;
    using TemplateAndExits = ValueTuple<List<(int X, int Y)>, List<(int X, int Y)>>;
    
    internal static class RoomTemplates
    {
        
        public static TemplateAndExits Square = (
            new XYList { (-2, -2), (2, -2), (2, 2), (-2, 2) },
            new XYList { (0, -2), (-2, 0), (0, 2), (2, 0)}   
        );
        
        public static TemplateAndExits H = (
            new XYList { (-4, -4), (-4, 4), (-3, 4), (-3, 1), (3, 1), (3, 4), (4, 4), (4, -4), (3, -4), (3, -1), (-3, -1), (-3, -4) },
            new XYList { (0, -1), (0, 1) }
        );

        public static TemplateAndExits L = (
            new XYList { (-2, -4), (-2, 4), (2, 4), (2, 2), (0, 2), (0, -4) },
            new XYList { (1, 2) }
        );

        public static TemplateAndExits U = (
            new XYList { (-4, -4), (-4, 4), (4, 4), (4, -4), (2, -4), (2, 2), (-2, 2), (-2, -4) },
            new XYList { (-3, -4), (3, 4) }
        );
        
        public static TemplateAndExits Plus = (
            new XYList { (-1, -4), (-1, -1), (-4, -1), (-4, 1), (-1, 1), (-1, 4), (1, 4), (1, 1), (4, 1), (4, -1), (1, -1), (1, -4) },
            new XYList { (0, 4), (0, -4), (4, 0), (-4, 0) }
        );
        
        public static readonly TemplateAndExits[] AllTemplates =
        {
            Square, Plus, H, L, U
        };

        public static (int minX, int minY, int maxX, int maxY) MinMax(this XYIEnumerable template)
        {
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;
            foreach (var (x, y) in template)
            {
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
            return (minX, minY, maxX, maxY);
        }
        
        public static TemplateAndExits Scale(this TemplateAndExits te, double amount) =>
            (te.Item1.Scale(amount).ToList(), te.Item2.Scale(amount).ToList());
        
        public static TemplateAndExits Translate(this TemplateAndExits te, (int X, int Y) amount) =>
            (te.Item1.Translate(amount).ToList(), te.Item2.Translate(amount).ToList());
        
        public static TemplateAndExits Flip(this TemplateAndExits te, bool x, bool y) =>
            (te.Item1.Flip(x, y).ToList(), te.Item2.Flip(x, y).ToList());
        
        public static TemplateAndExits Rotate(this TemplateAndExits te) =>
            (te.Item1.Rotate().ToList(), te.Item2.Rotate().ToList());
        
        public static XYIEnumerable Scale(this XYIEnumerable template, double amount) =>
            template.Select(point => point.Scale(amount));

        public static XYIEnumerable Translate(this XYIEnumerable template, (int X, int Y) amount) => 
            template.Select(point => point.Translate(amount));

        public static XYIEnumerable Flip(this XYIEnumerable template, bool x, bool y) =>
            template.Select(point => point.Flip(x, y));

        public static XYIEnumerable Rotate(this XYIEnumerable template) =>
            template.Select(point => point.Rotate());
        
        public static (int X, int Y) Scale(this (int X, int Y) point, double amount) =>
           ((int) (point.X * amount), (int) (point.Y * amount));

        public static (int X, int Y) Translate(this (int X, int Y) point, (int X, int Y) amount) => 
           (point.X + amount.X, point.Y + amount.Y);

        public static (int X, int Y) Flip(this (int X, int Y) point, bool x, bool y) =>
           (x ? -point.X : point.X, y ? -point.Y : point.Y);

        public static (int X, int Y) Rotate(this (int X, int Y) point) =>
           (point.Y, point.X);

    }
}