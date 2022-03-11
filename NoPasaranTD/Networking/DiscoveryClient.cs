using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NoPasaranTD.Networking
{
    public class DiscoveryClient : IDisposable
    {
        #region Property region
        /// <summary>
        /// Gibt an ob die derzeit besuchte Lobby gestartet wurde
        /// </summary>
        public bool GameStarted { get; private set; } = false;

        /// <summary>
        /// Gibt an ob sich der Client authentifiziert hat
        /// </summary>
        public bool LoggedIn { get; private set; } = false;

        /// <summary>
        /// Gibt die UDP Verbindung an, die ursprünglich als null-Referenz gilt.<br/>
        /// Geändert wird dies sobald eine Peer2Peer Verbindung aufgebaut wurde,
        /// weshalb man dies wirklich nur benutzen sollte wenn GameStarted = true ist
        /// </summary>
        public UdpClient UdpClient { get; private set; }

        /// <summary>
        /// Gibt eine liste an NetworkClients die sich im Spiel befinden, die ursprünglich als null-Referenz gilt.<br/>
        /// Geändert wird dies sobald eine Peer2Peer Verbindung aufgebaut wurde,
        /// weshalb man dies wirklich nur benutzen sollte wenn GameStarted = true ist
        /// </summary>
        public List<NetworkClient> Clients { get; private set; }

        /// <summary>
        /// Gibt die verfügbaren Lobbies an. Dies sollte nur genutzt werden, wenn LoggedIn = true ist
        /// </summary>
        public List<NetworkLobby> Lobbies { get; }
        #endregion

        private Socket TcpClient { get; }
        private StreamWriter TcpWriter { get; }
        private StreamReader TcpReader { get; }
        public DiscoveryClient(string address, int port)
        {
            Lobbies = new List<NetworkLobby>();
            TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TcpClient.Connect(address, port);

            TcpWriter = new StreamWriter(new NetworkStream(TcpClient));
            TcpReader = new StreamReader(new NetworkStream(TcpClient));
            new Thread(ListenCommands).Start();
        }

        #region Command region
        /// <summary>
        /// Authentifiziert sich als den gegebenen Spieler<br/>
        /// Ab hier fängt LoggedIn an sich auf "true" zu setzen
        /// </summary>
        /// <param name="player">Der spieler als den man sich authentifiziert</param>
        public async void LoginAsync(NetworkClient player)
        {
            string playerInfo = NetworkClient.Serialize(player);
            await TcpWriter.WriteLineAsync(playerInfo);
            await TcpWriter.FlushAsync();
        }

        /// <summary>
        /// Aktualisiert die Informationen des einzelnen Spielers
        /// </summary>
        /// <param name="player">Spezifizierter Spieler</param>
        public async void UpdatePlayer(NetworkClient player)
        {
            string playerInfo = NetworkClient.Serialize(player);
            await TcpWriter.WriteLineAsync("SetUserInfo#" + playerInfo);
            await TcpWriter.FlushAsync();
        }

        /// <summary>
        /// Erstellt eine neue Lobby indem man auch direkt hinzugefügt wird
        /// </summary>
        /// <param name="lobby">Spezifizierte Lobby</param>
        public async void CreateLobby(NetworkLobby lobby)
        {
            await TcpWriter.WriteLineAsync("NewLobby#" + lobby.GetInfo());
            await TcpWriter.FlushAsync();
        }

        /// <summary>
        /// Aktualisiert die gegebene Lobbyinformationen
        /// </summary>
        /// <param name="lobby">Spezifizierte Lobby</param>
        public async void UpdateLobby(NetworkLobby lobby)
        {
            await TcpWriter.WriteLineAsync("SetLobbyInfo#" + lobby.GetInfo());
            await TcpWriter.FlushAsync();
        }

        /// <summary>
        /// Tritt einer Lobby bei
        /// </summary>
        /// <param name="lobby">Spezifizierte Lobby</param>
        public async void JoinLobby(NetworkLobby lobby)
        {
            await TcpWriter.WriteLineAsync("Join#" + lobby.GetInfo());
            await TcpWriter.FlushAsync();
        }

        /// <summary>
        /// Verlässt die gerade beigetretene Lobby
        /// </summary>
        public async void LeaveCurrentLobby()
        {
            await TcpWriter.WriteLineAsync("Leave#");
            await TcpWriter.FlushAsync();
        }

        /// <summary>
        /// Gibt den Befehl frei, das Spiel zu starten.<br/>
        /// Ab hier fängt GameStarted an sich auf "true" zu setzen
        /// </summary>
        public async void StartGame()
        {
            await TcpWriter.WriteLineAsync("StartGame#");
            await TcpWriter.FlushAsync();
        }
        #endregion

        #region Input region
        private void HandleStartP2P(string infoStr)
        {
            string[] infoArgs = infoStr.Split('|');
            int port = int.Parse(infoArgs[0]);
            int id = int.Parse(infoArgs[1]);

            // Erstelle Serverendpunkt
            IPEndPoint remoteEndpoint = new IPEndPoint(
                (TcpClient.RemoteEndPoint as IPEndPoint).Address, port
            );
            UdpClient = new UdpClient();

            // Sende auf Serverendpunkt die gegebene ID
            byte[] data = Encoding.ASCII.GetBytes(id.ToString());
            UdpClient.Send(data, data.Length, remoteEndpoint);

            string connectionStr = TcpReader.ReadLine();
            string[] connectionArgs = connectionStr.Split('|');

            Clients = new List<NetworkClient>();
            for (int i = 0; i < connectionArgs.Length / 2; i++)
            { // Serialisiere die Antwort vom Server
                NetworkClient client = NetworkClient.Deserialize(connectionArgs[i]);
                string[] endpointArgs = connectionArgs[i + 1].Split(':');
                client.EndPoint = new IPEndPoint(
                    IPAddress.Parse(endpointArgs[0]), int.Parse(endpointArgs[1])
                );
                Clients.Add(client);
            }

            GameStarted = true;
        }

        private void HandleInfo(string infoStr)
        { // Aktualisiere die Lobbies
            Lobbies.Clear();
            string[] lobbyInfos = infoStr.Split('\t');
            for (int i = 0; i < lobbyInfos.Length; i++)
                Lobbies.Add(NetworkLobby.Deserialize(lobbyInfos[i]));
            LoggedIn = true;
        }

        private void ListenCommands()
        {
            try
            {
                while (true)
                {
                    // Warte auf Kommando vom Server
                    string message = TcpReader.ReadLine();
                    if (message == null) throw new IOException("Stream was closed");

                    int index = message.IndexOf('#');
                    if (index == -1)
                    {
                        Console.WriteLine("Failed to parse message: " + message);
                        continue;
                    }
                    string command = message.Substring(0, index);
                    string paramStr = message.Substring(index + 1);

                    switch (command)
                    { // Verarbeite das Kommando vom Server
                        case "Info":
                            HandleInfo(paramStr);
                            break;
                        case "StartP2P":
                            HandleStartP2P(paramStr);
                            break;
                        default:
                            Console.WriteLine("Unknown command: " + message);
                            break;
                    }
                }
            }
            catch { }
        }
        #endregion

        public void Dispose()
        {
            TcpWriter.Dispose();
            TcpReader.Dispose();
            TcpClient.Dispose();
        }
    }
}
