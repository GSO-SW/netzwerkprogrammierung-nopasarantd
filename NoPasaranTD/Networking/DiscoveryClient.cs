using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace NoPasaranTD.Networking
{
    public class DiscoveryClient : IDisposable
    {

        public List<NetworkLobby> Lobbies { get; }
        public bool LoggedIn { get; private set; }

        private Socket TcpClient { get; }
        private StreamWriter Writer { get; }
        public DiscoveryClient(string address, int port)
        {
            Lobbies = new List<NetworkLobby>();
            TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TcpClient.Connect(address, port);

            Writer = new StreamWriter(new NetworkStream(TcpClient));
            new Thread(ListenCommands).Start();
        }

        public async void LoginAsync(NetworkClient player)
        {
            string playerInfo = NetworkClient.Deserialize(player);
            await Writer.WriteLineAsync(playerInfo);
            await Writer.FlushAsync();
        }

        #region Command region
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
                using (StreamReader reader = new StreamReader(new NetworkStream(TcpClient)))
                {
                    while (true)
                    {
                        string message = reader.ReadLine();

                        int index = message.IndexOf('#');
                        if (index == -1)
                        {
                            Console.WriteLine("Failed to parse message: " + message);
                            continue;
                        }
                        string command = message.Substring(0, index);
                        string paramString = message.Substring(index + 1);

                        switch (command)
                        {
                            case "Info":
                                HandleInfo(paramString);
                                break;
                        }
                    }
                }
            }
            catch { }
        }
        #endregion

        public void Dispose()
        {
            Writer.Dispose();
            TcpClient.Dispose();
        }
    }
}
