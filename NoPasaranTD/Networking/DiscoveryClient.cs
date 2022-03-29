﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoPasaranTD.Networking
{
    public class DiscoveryClient : IDisposable
    {
        #region Property region
        /// <summary>
        /// Wird aufgerufen, sobald das Spiel gestartet wird
        /// </summary>
        public Action OnGameStart { get; set; }

        /// <summary>
        /// Wird aufgerufen, sobald eine Information aktualisiert wurde
        /// </summary>
        public Action OnInfoUpdate { get; set; }

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
        public RUdpClient UdpClient { get; private set; }

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
            LoggedIn = false;
            string playerInfo = NetworkClient.Serialize(player);
            await TcpWriter.WriteLineAsync(playerInfo);
            await TcpWriter.FlushAsync();
        }

        /// <summary>
        /// Aktualisiert die Informationen des einzelnen Spielers
        /// </summary>
        /// <param name="player">Spezifizierter Spieler</param>
        public async void UpdatePlayerAsync(NetworkClient player)
        {
            LoggedIn = false;
            string playerInfo = NetworkClient.Serialize(player);
            await TcpWriter.WriteLineAsync("SetUserInfo#" + playerInfo);
            await TcpWriter.FlushAsync();
        }

        /// <summary>
        /// Erstellt eine neue Lobby indem man auch direkt hinzugefügt wird
        /// </summary>
        /// <param name="lobby">Spezifizierte Lobby</param>
        public async void CreateLobbyAsync(NetworkLobby lobby)
        {
            await TcpWriter.WriteLineAsync("NewLobby#" + lobby.GetInfo());
            await TcpWriter.FlushAsync();
        }

        /// <summary>
        /// Aktualisiert die gegebene Lobbyinformationen
        /// </summary>
        /// <param name="lobby">Spezifizierte Lobby</param>
        public async void UpdateLobbyAsync(NetworkLobby lobby)
        {
            await TcpWriter.WriteLineAsync("SetLobbyInfo#" + lobby.GetInfo());
            await TcpWriter.FlushAsync();
        }

        /// <summary>
        /// Tritt einer Lobby bei
        /// </summary>
        /// <param name="lobby">Spezifizierte Lobby</param>
        public async void JoinLobbyAsync(NetworkLobby lobby)
        {
            await TcpWriter.WriteLineAsync("Join#" + lobby.GetInfo());
            await TcpWriter.FlushAsync();
        }

        /// <summary>
        /// Verlässt die gerade beigetretene Lobby
        /// </summary>
        public async void LeaveCurrentLobbyAsync()
        {
            await TcpWriter.WriteLineAsync("Leave#");
            await TcpWriter.FlushAsync();
        }

        /// <summary>
        /// Gibt den Befehl frei, das Spiel zu starten.<br/>
        /// Ab hier fängt GameStarted an sich auf "true" zu setzen
        /// </summary>
        public async void StartGameAsync()
        {
            GameStarted = false;
            await TcpWriter.WriteLineAsync("StartGame#");
            await TcpWriter.FlushAsync();
        }
        #endregion

        #region Hole-Punching region

        /// <summary>
        /// Startet den Udp-Hole-Punching prozess.
        /// Hierbei wird jedem Empfänger eine Anfrage gesendet, damit die Firewall die ausgehenden Verbindungen erkennt.
        /// Dies hat den Effekt, dass Sie die Daten die auf den lokalen Port des Sockets zulässt.
        /// Mehr Informationen über UDP/TCP-Hole-Punching hier: https://bford.info/pub/net/p2pnat/
        /// </summary>
        private async Task DoHolePunchingAsync()
        {
            IPEndPoint[] endpoints = Clients.Select(c => c.EndPoint).ToArray();
            await UdpClient.SendReliableAsync(new byte[0], endpoints); // Sende leeres Paket an alle Endpunkte

            // Warte auf Antwort von jedem Endpunkt
            for (int i = 0; i < endpoints.Length; i++)
            {
                await UdpClient.ReceiveAsync();
            }
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
            UdpClient rawClient = new UdpClient();
            rawClient.AllowNatTraversal(true);

            Task.Run(async () =>
            { // Stelle sicher, dass der Vermittlungsserver die Nachricht erreicht
                while (!GameStarted)
                {
                    // Sende auf Serverendpunkt die gegebene ID
                    byte[] data = Encoding.ASCII.GetBytes(id.ToString());
                    rawClient.Send(data, data.Length, remoteEndpoint);
                    await Task.Delay(1000);
                }
            });

            UdpClient = new RUdpClient(rawClient);
            Clients = new List<NetworkClient>();

            string connectionStr = TcpReader.ReadLine();
            if (!string.IsNullOrEmpty(connectionStr))
            { // Wenn es Endpunkte gibt
                string[] connectionArgs = connectionStr.Split('|');
                for (int i = 0; i < connectionArgs.Length; i += 2)
                { // Serialisiere die Antwort vom Server
                    NetworkClient client = NetworkClient.Deserialize(connectionArgs[i]);
                    string[] endpointArgs = connectionArgs[i + 1].Split(':');
                    client.EndPoint = new IPEndPoint(
                        IPAddress.Parse(endpointArgs[0]), int.Parse(endpointArgs[1])
                    );
                    Clients.Add(client);
                }
            }

            Task.WaitAll(DoHolePunchingAsync());

            GameStarted = true;
            // Rufe handler auf
            OnGameStart?.Invoke();
        }

        private void HandleInfo(string infoStr)
        { // Aktualisiere die Lobbies
            Lobbies.Clear();
            string[] lobbyInfos = infoStr.Split('\t');
            // int i = 1, aufgrund der Fake-Lobby
            for (int i = 1; i < lobbyInfos.Length; i++)
            {
                Lobbies.Add(NetworkLobby.Deserialize(lobbyInfos[i]));
            }

            LoggedIn = true;

            // Rufe handler auf
            OnInfoUpdate?.Invoke();
        }

        private void ListenCommands()
        {
            try
            {
                while (true)
                {
                    // Warte auf Kommando vom Server
                    string message = TcpReader.ReadLine();
                    if (message == null)
                    {
                        throw new IOException("Stream was closed");
                    }

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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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
