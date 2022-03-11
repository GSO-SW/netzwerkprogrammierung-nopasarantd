﻿using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Threading;
using System;

namespace NoPasaranMS
{
	static class UDPHolePunching
	{
		public static void Connect(List<Player> players, int port)
		{
			try
			{
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] sending 'StartP2P' to {new StringBuilder().AppendJoin(", ", players.Select(x => x.Socket.RemoteEndPoint))}");
				// setup local udp
				var localEndpoint = new IPEndPoint(IPAddress.Any, port);
				using var serverUdpSocket = new UdpClient(port);
				// each client gets send an id over tcp and sends it back over udp
				// send each client the above port and their id
				for (int i = 0; i < players.Count; i++)
					players[i].Writer.WriteLine("StartP2P#" + port + '|' + i);
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] receiving endpoints");
				// receive hello messages from clients and save their endpoints by id
				var clientUdpEndpoints = new IPEndPoint[players.Count];
				while (clientUdpEndpoints.Any(ep => ep == null))
				{
					var ep = new IPEndPoint(0, 0);
					int id = int.Parse(Encoding.ASCII.GetString(serverUdpSocket.Receive(ref ep)));
					clientUdpEndpoints[id] = ep;
				}
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] received: {new StringBuilder().AppendJoin<IPEndPoint>(", ", clientUdpEndpoints)}");
				Console.Write($"[{Thread.CurrentThread.ManagedThreadId}] sending out endpoints");

				for (int i = 0; i < players.Count; i++)
				{
					// pack endpoints into string "ep1|ep2|ep3"
					var sb = new StringBuilder();
					for (int j = 0; j < clientUdpEndpoints.Length; j++)
						if (i != j)
							sb.Append(clientUdpEndpoints[j]).Append('|');
					sb.Remove(sb.Length - 1, 1); // remove trailing '|'
					// send out the string
					players[i].Writer.WriteLine(sb);
					Console.Write('.');
				}
				// and we're done
				serverUdpSocket.Close();
				Console.WriteLine("Done");
			}
			catch (Exception e)
			{
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] couldn't start p2p connection");
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {e.Message}");
			}
			finally
			{
				foreach (var player in players)
					player.Socket.Close();
			}
		}
	}
}
