using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NoPasaranMS
{
	public class Lobby
	{
		public string Info;
		public List<Player> Players = new List<Player>();
		public Lobby(string info) => Info = info;
		public Lobby(string info, Player host)
		{
			Info = info;
			Players.Add(host);
		}
		public string FullInfo => new StringBuilder(Info + '|').AppendJoin('|', Players.Select(p => p.Info)).ToString();

	}

	public class Player
	{
		public string Info;
		public Socket Socket;
		public StreamWriter Writer;
		public Player(string info, Socket socket, StreamWriter writer)
		{
			Info = info;
			Socket = socket;
			Writer = writer;
		}
	}
}
