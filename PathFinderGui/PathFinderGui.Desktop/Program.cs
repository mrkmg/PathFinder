using System;
using Eto;
using Eto.Forms;
using Eto.Drawing;

namespace PathFinderGui.Desktop
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new Application(Eto.Platform.Get(Platforms.Gtk)).Run(new MainForm());
        }
    }
}