using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NoPasaranMS
{
    public class Lobby
    {
        public string Info { get; set; }

        public readonly List<Player> Players = new List<Player>();
        public Lobby(string info)
        {
            Info = info;
        }

        public Lobby(string info, Player host)
        {
            Info = info;
            Players.Add(host);
        }

        /// <summary>
        /// Generiert eine Zeichenkette mit allen Informationen, die die Lobby zur verfügung hat
        /// </summary>
        public string FullInfo => new StringBuilder(Info + '|').AppendJoin('|', Players.Select(p => p.Info)).ToString();
    }

    public class Player
    {
        public string Info { get; set; }

        public readonly Socket Socket;
        public readonly StreamWriter Writer;
        public Player(string info, Socket socket, StreamWriter writer)
        {
            Info = info;
            Socket = socket;
            Writer = writer;
        }
    }
}
