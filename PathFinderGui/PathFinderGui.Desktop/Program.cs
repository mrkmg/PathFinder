using System;
using Eto.Forms;

namespace PathFinderGui.Desktop
{
    class Program
    {
        // ReSharper disable once UnusedParameter.Local
        [STAThread]
        static void Main(string[] args)
        {
            new Application(Eto.Platform.Detect).Run(new MainForm());
        }
    }
}