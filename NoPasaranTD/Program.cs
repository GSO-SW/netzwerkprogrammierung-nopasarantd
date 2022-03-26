﻿using NoPasaranTD.Engine;
using NoPasaranTD.Networking;
using NoPasaranTD.Visuals;
using System;
using System.Windows.Forms;

namespace NoPasaranTD
{
    internal static class Program
    {
        public const string SERVER_ADDRESS = "85.214.40.156";
        public const int SERVER_PORT = 31415;

        private static StaticDisplay display;

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            {
                display = new StaticDisplay();
                display.LoadGame(null);
            }
            Application.Run(display);
        }

        public static void LoadGame(string mapFile)
        {
            display.LoadGame(mapFile);
        }

        public static void LoadGame(string mapFile, NetworkHandler networkHandler)
        {
            display.LoadGame(mapFile, networkHandler);
        }

        public static void LoadScreen(GuiComponent screen)
        {
            display.LoadScreen(screen);
        }
    }
}
