using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;

namespace NoPasaranMS
{
	public class DiscoveryServer
	{
		private readonly int Port;
		private readonly Action<List<Socket>> GroupFoundCallback;
		private readonly List<Lobby> Lobbies = new List<Lobby>();

		public DiscoveryServer(int port, Action<List<Socket>> groupFoundCallback)
		{
			Port = port;
			GroupFoundCallback = groupFoundCallback;
			Lobbies.Add(new Lobby("FreePlayers"));
		}

		public void Run()
		{
			IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, Port);
			Socket serverSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			serverSocket.Bind(endpoint);
			serverSocket.Listen(16);
			while (true)
			{
				var clientSocket = serverSocket.Accept();
				new Thread(() => Receive(clientSocket)).Start();
			}
		}
		private void Receive(Socket clientSocket)
		{
			clientSocket.SendTimeout = 100;
			Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] new connection from {clientSocket.RemoteEndPoint}");
			Player p = null;
			try
			{
				using var networkStream = new NetworkStream(clientSocket);
				using var writer = new StreamWriter(networkStream);
				using var reader = new StreamReader(networkStream);
				string playerInfo = reader.ReadLine();
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] added player {playerInfo}");
				p = new Player(playerInfo, clientSocket, writer);
				Lobbies[0].Players.Add(p);
				SendUpdates();
				while (!p.StopReceiving.WaitOne(100))
					if (clientSocket.Available > 0)
						HandleMessage(p, reader.ReadLine());
			}
			catch (Exception)
			{
				if (p != null)
				{
					Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] dropping {clientSocket.RemoteEndPoint} '{p.Info}'");
					RemovePlayerFromLobby(p);
				}
				else
					Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] dropping {clientSocket.RemoteEndPoint}");
			}
		}
		private void HandleMessage(Player sender, string message)
		{
			try
			{
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] handling message '{message}' from {sender.Info}");
				string type = message[..message.IndexOf('#')];
				string content = message[(message.IndexOf('#') + 1)..];
				Lobby lobby = Lobbies.Where(l => l.Players.Contains(sender)).FirstOrDefault();
				switch (type)
				{
					case "SetUserInfo": 
						Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {sender.Info} send new user info '{content}'");
						sender.Info = content;
						break;
					case "SetLobbyInfo": 
						Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {sender.Info} send new lobby info for {lobby.Info} -> '{content}'");
						lobby.Info = content;
						break;
					case "StartGame":
						Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {sender.Info} started game for lobby {lobby.Info}");
						Lobbies.Remove(lobby);
						foreach (var p in lobby.Players)
							p.StopReceiving.Set();
						GroupFoundCallback(lobby.Players.Select(p => p.Socket).ToList());
						break;
					case "Join":
						Lobbies.Find(l => l.Info == content).Players.Add(sender);
						RemovePlayerFromLobby(sender);
						Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {sender.Info} joined {content}");
						break;
					case "Leave":
						RemovePlayerFromLobby(sender);
						Lobbies[0].Players.Add(sender);
						if (lobby.Players.Count == 0)
							Lobbies.Remove(lobby);
						Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {sender.Info} left {content}");
						break;
					case "NewLobby":
						Lobbies.Add(new Lobby(content, sender));
						RemovePlayerFromLobby(sender);
						Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {sender.Info} created lobby {content}");
						break;
				}
				SendUpdates();
			}
			catch (Exception)
			{
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] weird message from {sender.Info}");
			}
		}
		private void SendUpdates()
		{
			Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] sending updates -> {FullInfo()}");
			string info = FullInfo();
			for (int i = 0; i < Lobbies.Count; i++)
			{
				Lobby l = Lobbies[i];
				for (int j = 0; j < l.Players.Count; j++)
				{
					Player p = l.Players[j];
					try
					{
						p.Writer.WriteLine("Info#" + info);
						p.Writer.Flush();
					}
					catch (Exception)
					{
						RemovePlayerFromLobby(p);
						p.StopReceiving.Set();
					}
				}
			}
		}
		private void RemovePlayerFromLobby(Player p)
		{
			var i = Lobbies.FindIndex(l => l.Players.Contains(p));
			Lobbies[i].Players.Remove(p);
			if (i > 0 && Lobbies[i].Players.Count == 0)
			{
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] removed lobby {Lobbies[i].Info}");
				Lobbies.RemoveAt(i);
			}
		}

		private string FullInfo() => new StringBuilder().AppendJoin('\t', Lobbies.Select(l => l.FullInfo)).ToString();

		private class Lobby
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
		private class Player
		{			
			public string Info;
			public Socket Socket;
			public StreamWriter Writer;
			public ManualResetEvent StopReceiving = new ManualResetEvent(false);
			public Player(string info, Socket socket, StreamWriter writer)
			{
				Info = info;
				Socket = socket;
				Writer = writer;
			}
		}
	}
}
