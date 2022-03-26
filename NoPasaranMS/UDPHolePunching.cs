using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NoPasaranMS
{
    internal static class UDPHolePunching
    {
        /// <summary>
        /// Vermittelt jedem Spieler die UDP-Endpunkte um UDP-Hole-Punching zu ermöglichen
        /// </summary>
        public static void Connect(List<Player> players, int port)
        {
            try
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] sending 'StartP2P' to {new StringBuilder().AppendJoin(", ", players.Select(x => x.Socket.RemoteEndPoint))}");
                // setup local udp
                IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, port);
                using (UdpClient serverUdpSocket = new UdpClient(port))
                {
                    // each client gets send an id over tcp and sends it back over udp
                    // send each client the above port and their id
                    for (int i = 0; i < players.Count; i++)
                    {
                        players[i].Writer.WriteLine("StartP2P#" + port + '|' + i);
                        players[i].Writer.Flush();
                    }

                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] receiving endpoints");
                    // receive hello messages from clients and save their endpoints by id
                    IPEndPoint[] clientUdpEndpoints = new IPEndPoint[players.Count];
                    while (clientUdpEndpoints.Any(ep => ep == null))
                    {
                        IPEndPoint ep = new IPEndPoint(0, 0);
                        int id = int.Parse(Encoding.ASCII.GetString(serverUdpSocket.Receive(ref ep)));
                        clientUdpEndpoints[id] = ep;
                    }
                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] received: {new StringBuilder().AppendJoin<IPEndPoint>(", ", clientUdpEndpoints)}");
                    Console.Write($"[{Thread.CurrentThread.ManagedThreadId}] sending out endpoints");

                    for (int i = 0; i < players.Count; i++)
                    {
                        // pack endpoints into string "info1|ep1|info2|ep2|info3|ep3"
                        StringBuilder sb = new StringBuilder();
                        for (int j = 0; j < clientUdpEndpoints.Length; j++)
                        {
                            if (i != j)
                            {
                                sb.Append(players[j].Info).Append('|')
                                    .Append(clientUdpEndpoints[j]).Append('|');
                            }
                        }

                        if (sb.Length > 0) // sb.Length = 0, solange sich nur 1 Spieler in der Lobby befindet
                        {
                            sb.Remove(sb.Length - 1, 1); // remove trailing '|'
                        }

                        // send out the string
                        players[i].Writer.WriteLine(sb);
                        players[i].Writer.Flush();
                        Console.Write('.');
                    }
                    // and we're done
                    Console.WriteLine("Done");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] couldn't start p2p connection");
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {e.Message}");
            }
            finally
            {
                foreach (Player player in players)
                {
                    player.Socket.Close();
                }
            }
        }
    }
}
