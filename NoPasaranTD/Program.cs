using System;
using System.Windows.Forms;
using NoPasaranTD.Engine;

namespace NoPasaranTD
{
    internal static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StaticDisplay());
        }
    }
}
