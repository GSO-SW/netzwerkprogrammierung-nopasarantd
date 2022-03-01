using System.Collections.Generic;
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
		public static void Connect(List<Socket> tcpClients, int port)
		{
			try
			{
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] sending 'StartP2P' to {new StringBuilder().AppendJoin(", ", tcpClients.Select(x => x.RemoteEndPoint))}");
				var localEndpoint = new IPEndPoint(IPAddress.Any, port);
				using var udp = new UdpClient(port);
				for (int i = 0; i < tcpClients.Count; i++)
					tcpClients[i].Send(Encoding.ASCII.GetBytes("StartP2P#" + port + '|' + i));
				var clientEndpoints = new IPEndPoint[tcpClients.Count];
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] receiving endpoints");
				for (int i = 0; i < tcpClients.Count; i++)
				{
					var ep = new IPEndPoint(0, 0);
					int id = int.Parse(Encoding.ASCII.GetString(udp.Receive(ref ep)));
					clientEndpoints[id] = ep;
				}
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] received: {new StringBuilder().AppendJoin<IPEndPoint>(", ", clientEndpoints)}");
				Console.Write($"[{Thread.CurrentThread.ManagedThreadId}] sending out endpoints");
				for (int i = 0; i < tcpClients.Count; i++)
				{
					var sb = new StringBuilder();
					for (int j = 0; j < clientEndpoints.Length; j++)
						if (i != j)
							sb.Append(clientEndpoints[j]).Append('|');
					sb.Remove(sb.Length - 1, 1); // remove trailing '|'
					tcpClients[i].Send(Encoding.ASCII.GetBytes(sb.ToString()));
					Console.Write('.');
				}
				udp.Close();
				Console.WriteLine("Done");
			}
			catch (Exception e)
			{
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] couldn't start p2p connection");
				Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {e.Message}");
			}
			finally
			{
				foreach (var socket in tcpClients)
					socket.Close();
			}
		}
	}
}
