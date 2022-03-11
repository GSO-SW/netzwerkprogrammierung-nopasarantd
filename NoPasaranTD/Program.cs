﻿using System;
using System.Windows.Forms;
using NoPasaranTD.Engine;
using NoPasaranTD.Networking;
using NoPasaranTD.Visuals;

namespace NoPasaranTD
{
    internal static class Program
    {
        private static StaticDisplay display;

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (DiscoveryClient client = new DiscoveryClient("127.0.0.1", 31415))
            {
                NetworkClient localPlayer = new NetworkClient("Paolo V.");

                client.LoginAsync(localPlayer);
                while (!client.LoggedIn) ;

                NetworkLobby localLobby = new NetworkLobby(localPlayer, "Test Lobby");
                client.CreateLobby(localLobby);
                client.StartGame();

                while (!client.GameStarted) ;

                foreach(NetworkClient networkClient in client.Clients)
                    Console.WriteLine(networkClient.Name + ", " + networkClient.EndPoint);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            {
                display = new StaticDisplay();
                display.LoadGame(null);
            }
            Application.Run(display);
        }

        public static void LoadGame(string mapFile)
            => display.LoadGame(mapFile);

        public static void LoadScreen(GuiComponent screen)
            => display.LoadScreen(screen);
    }
}
