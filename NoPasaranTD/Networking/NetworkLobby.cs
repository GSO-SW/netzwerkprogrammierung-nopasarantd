﻿using System;
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

        public string MapName { get; set; }
        public string Name { get; set; }

        public NetworkLobby(NetworkClient host, string name, string mapname)
        {
            Players = new List<NetworkClient>();
            Name = name;
            Host = host;
            MapName = mapname;
        }

        public string GetInfo() => $"{Name}#{MapName}";

        /// <summary>
        /// Sucht nach einem Spieler in dieser Lobby,
        /// dessen serialisierten Version, dem vom gegebenen entspricht
        /// </summary>
        /// <param name="client">Der zu suchende Spieler</param>
        /// <returns>Ob der gegebene Spieler gefunden wurde</returns>
        public bool PlayerExists(NetworkClient client)
        {
            if (client == null) return false;
            string serializedClient = NetworkClient.Serialize(client);
            foreach(NetworkClient player in Players)
            {
                string serializedPlayer = NetworkClient.Serialize(player);
                if (serializedPlayer.Equals(serializedClient))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Serialisiert eine NetworkLobby zu einer Zeichenkette,
        /// die vom Vermittlungsserver ausgewertet werden kann.
        /// </summary>
        /// <param name="lobby">Die zu serialisierende NetworkLobby</param>
        /// <returns>Die serialisierte Zeichenkette</returns>
        public static string Serialize(NetworkLobby lobby)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(lobby.GetInfo());
            foreach(NetworkClient client in lobby.Players)
            {
                string playerInfo = NetworkClient.Serialize(client);
                builder.Append('|').Append(playerInfo);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Deserialisiert eine Zeichenkette zu einer NetworkLobby,
        /// die vom Spiel ausgewertet werden kann.
        /// </summary>
        /// <param name="lobby">Die zu deserialisierende Zeichenkette</param>
        /// <returns>Die deserialisierte NetworkLobby</returns>
        public static NetworkLobby Deserialize(string info)
        {
            int index = info.IndexOf('|');
            if (index == -1) throw new Exception("Could not parse server string");

            string lobbyInfo = info.Substring(0, index);
            string playerInfo = info.Substring(index + 1);

            string[] playerInfos = playerInfo.Split('|');
            
            if (playerInfos.Length == 0 || string.IsNullOrEmpty(playerInfos[0]))
                throw new Exception("Received an empty lobby");

            NetworkClient host = NetworkClient.Deserialize(playerInfos[0]);

            string[] lobbyInfos = lobbyInfo.Split('#');
            NetworkLobby lobby = new NetworkLobby(host, lobbyInfos[0], lobbyInfos[1]);

            // int i = 1 aufgrund vom host, der schon definiert wird
            for (int i = 1; i < playerInfos.Length; i++)
                lobby.Players.Add(NetworkClient.Deserialize(playerInfos[i]));
            return lobby;
        }
    }
}
