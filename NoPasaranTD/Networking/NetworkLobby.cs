using System;
using System.Collections.Generic;
using System.Text;

namespace NoPasaranTD.Networking
{
    public class NetworkLobby
    {
        public List<NetworkClient> Players { get; }

        private NetworkClient host;
        public NetworkClient Host 
        {
            get { return host; }
            set
            {
                host = value;
                if (Players.Count == 0) Players.Add(value);
                else Players[0] = value;
            }
        }

        public string Name { get; set; }

        public NetworkLobby(NetworkClient host, string name)
        {
            Players = new List<NetworkClient>();
            Name = name;
            Host = host;
        }

        public string GetInfo() => Name;

        public static string Deserialize(NetworkLobby lobby)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(lobby.GetInfo());
            foreach(NetworkClient client in lobby.Players)
            {
                string playerInfo = NetworkClient.Deserialize(client);
                builder.Append('|').Append(playerInfo);
            }

            return builder.ToString();
        }

        public static NetworkLobby Serialize(string info)
        {
            int index = info.IndexOf('|');
            if (index == -1) throw new Exception("Could not parse server string");

            string lobbyInfo = info.Substring(0, index);
            string playerInfo = info.Substring(index + 1);

            string[] playerInfos = playerInfo.Split('|');
            // Probably receiving the fake lobby, which can indeed be empty?
            if (playerInfos.Length == 0) throw new Exception("Received an empty lobby");

            NetworkClient host = NetworkClient.Serialize(playerInfos[0]);
            NetworkLobby lobby = new NetworkLobby(host, lobbyInfo);

            // int i = 1, because we are ignoring the Host which is already defined
            for (int i = 1; i < playerInfos.Length; i++)
                lobby.Players.Add(NetworkClient.Serialize(playerInfos[i]));
            return lobby;
        }
    }
}
