using System;
using System.Threading;
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
            new Thread(() =>
            {
                using (DiscoveryClient client = new DiscoveryClient("127.0.0.1", 31415))
                {
                    NetworkClient localPlayer = new NetworkClient("Paolo V.");
                    client.LoginAsync(localPlayer);
                    while (!client.LoggedIn) ;

                    client.CreateLobby(new NetworkLobby(localPlayer, "Test Lobby"));

                    while (!client.GameStarted) ;
                    foreach (NetworkClient c in client.Clients)
                        Console.WriteLine(c.Name + ", " + c.EndPoint);
                    while (true) ;
                }
            }).Start();
            Thread.Sleep(5000);
            new Thread(() =>
            {
                using (DiscoveryClient client = new DiscoveryClient("127.0.0.1", 31415))
                {
                    client.LoginAsync(new NetworkClient("Paolo V2."));
                    while (!client.LoggedIn) ;

                    client.JoinLobby(client.Lobbies[1]);
                    client.StartGame();

                    while (!client.GameStarted) ;
                    foreach (NetworkClient c in client.Clients)
                        Console.WriteLine(c.Name + ", " + c.EndPoint);
                    while (true) ;
                }
            }).Start();

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
