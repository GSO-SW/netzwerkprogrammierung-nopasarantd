using NoPasaranTD.Engine;
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

        /// <summary>
        /// Lade Spielinstanz im Offlinemodus.<br/>
        /// Falls der Parameter eine Null-Referenz ist, 
        /// entlädt er das Spiel und kehrt automatisch ins Hauptmenü zurück
        /// </summary>
        /// <param name="mapFile">Dateiname der Map</param>
        public static void LoadGame(string mapFile)
        {
            display.LoadGame(mapFile);
        }

        /// <summary>
        /// Lade Spielinstanz im Onlinemodus.<br/>
        /// Falls der Parameter eine Null-Referenz ist, 
        /// entlädt er das Spiel und kehrt automatisch ins Hauptmenü zurück
        /// </summary>
        /// <param name="mapFile">Dateiname der Map</param>
        /// <param name="handler">Dementsprechender Netzwerkmanager</param>
        public static void LoadGame(string mapFile, NetworkHandler networkHandler)
        {
            display.LoadGame(mapFile, networkHandler);
        }

        /// <summary>
        /// Lade einen überlappenden Screen<br/>
        /// Achtung: Das Spiel wird dabei nicht automatisch gestoppt, 
        /// sondern läuft im Hintergrund weiter!
        /// </summary>
        /// <param name="screen">Der zu ladende Screen</param>
        public static void LoadScreen(GuiComponent screen)
        {
            display.LoadScreen(screen);
        }
    }
}
