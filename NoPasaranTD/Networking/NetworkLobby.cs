using System;
using System.Collections.Generic;
using System.Text;

namespace NoPasaranTD.Networking
{
    public class NetworkLobby
    {
        public string Name { get; private set; }
        public List<NetworkClient> Players { get; private set; }
        public NetworkClient Host { get; private set; }

        private NetworkLobby() { }

        public static string Deserialize(NetworkLobby lobby)
        {
            StringBuilder builder = new StringBuilder();

            string lobbyInfo = lobby.Name;
            builder.Append(lobbyInfo);

            foreach(NetworkClient client in lobby.Players)
            {
                string playerInfo = NetworkClient.Deserialize(client);
                builder.Append('|').Append(playerInfo);
            }

            return builder.ToString();
        }

        public static NetworkLobby Serialize(string info)
        {
            NetworkLobby lobby = new NetworkLobby();
            { // Lobby serialisieren aus infoString
                int index = info.IndexOf('|');
                if (index == -1) throw new Exception("Could not parse server string");

                string lobbyInfo = info.Substring(0, index);
                string playerInfo = info.Substring(index + 1);

                lobby.Name = lobbyInfo;
                lobby.Players = new List<NetworkClient>();

                string[] playerInfos = playerInfo.Split('|');
                // Probably receiving the fake lobby, which can indeed be empty?
                if (playerInfos.Length == 0) throw new Exception("Received an empty lobby");

                for (int i = 0; i < playerInfos.Length; i++)
                    lobby.Players.Add(NetworkClient.Serialize(playerInfos[i]));
                lobby.Host = lobby.Players[0];
            }
            return lobby;
        }
    }
}
