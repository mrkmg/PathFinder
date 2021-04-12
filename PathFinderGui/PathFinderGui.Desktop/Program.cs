using System;
using Eto.Forms;

namespace PathFinderGui.Desktop
{
    internal static class Program
    {
        // ReSharper disable once UnusedParameter.Local
        [STAThread]
        private static void Main(string[] args)
        {
            new Application(Eto.Platform.Detect).Run(new MainForm());
        }
    }
}