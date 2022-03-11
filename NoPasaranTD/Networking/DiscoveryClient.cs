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
        public bool GameStarted { get; private set; } = false;
        public bool LoggedIn { get; private set; } = false;

        // Bleibt null bis eine Peer2Peer Verbindung aufgebaut wurde (GameStarted = true)
        public UdpClient UdpClient { get; private set; }
        public List<NetworkClient> Clients { get; private set; }

        public List<NetworkLobby> Lobbies { get; }

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
        public async void LoginAsync(NetworkClient player)
        {
            string playerInfo = NetworkClient.Deserialize(player);
            await TcpWriter.WriteLineAsync(playerInfo);
            await TcpWriter.FlushAsync();
        }

        public async void UpdatePlayer(NetworkClient player)
        {
            string playerInfo = NetworkClient.Deserialize(player);
            await TcpWriter.WriteLineAsync("SetUserInfo#" + playerInfo);
            await TcpWriter.FlushAsync();
        }

        public async void CreateLobby(NetworkLobby lobby)
        {
            await TcpWriter.WriteLineAsync("NewLobby#" + lobby.GetInfo());
            await TcpWriter.FlushAsync();
        }

        public async void UpdateLobby(NetworkLobby lobby)
        {
            await TcpWriter.WriteLineAsync("SetLobbyInfo#" + lobby.GetInfo());
            await TcpWriter.FlushAsync();
        }

        public async void JoinLobby(NetworkLobby lobby)
        {
            await TcpWriter.WriteLineAsync("Join#" + lobby.GetInfo());
            await TcpWriter.FlushAsync();
        }

        public async void LeaveCurrentLobby()
        {
            await TcpWriter.WriteLineAsync("Leave#");
            await TcpWriter.FlushAsync();
        }

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
            {
                NetworkClient client = NetworkClient.Serialize(connectionArgs[i]);
                string[] endpointArgs = connectionArgs[i + 1].Split(':');
                client.EndPoint = new IPEndPoint(
                    IPAddress.Parse(endpointArgs[0]), int.Parse(endpointArgs[1])
                );
                Clients.Add(client);
            }

            GameStarted = true;
        }

        private void HandleInfo(string infoStr)
        {
            Lobbies.Clear();
            string[] lobbyInfos = infoStr.Split('\t');
            for (int i = 0; i < lobbyInfos.Length; i++)
                Lobbies.Add(NetworkLobby.Serialize(lobbyInfos[i]));
            LoggedIn = true;
        }

        private void ListenCommands()
        {
            try
            {
                while (true)
                {
                    string message = TcpReader.ReadLine();

                    int index = message.IndexOf('#');
                    if (index == -1)
                    {
                        Console.WriteLine("Failed to parse message: " + message);
                        continue;
                    }
                    string command = message.Substring(0, index);
                    string paramStr = message.Substring(index + 1);

                    switch (command)
                    {
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
            TcpClient.Dispose();
        }
    }
}
