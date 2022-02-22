using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NoPasaranMS
{
    internal class Program
    {

        static void Main()
        {
            new Thread(ServerThread).Start();

            new Thread(ClientThread).Start();
            new Thread(ClientThread).Start();
        }

        /// <summary>
        /// Startet den hole punched Server (TCP)
        /// </summary>
        /// <param name="endpoint"></param>
        public static void PunchThread(object endpoint)
        {
            using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            { // Starte hole punched Server...
                server.Bind((IPEndPoint)endpoint);
                server.Listen(1);

                using (Socket client = server.Accept())
                using (NetworkStream stream = new NetworkStream(client))
                {
                    Console.WriteLine($"[Hole-Punched-Server]: Verbindung aufgebaut [l:{endpoint}; r:{client.RemoteEndPoint}]");

                    byte[] data = new byte[256];
                    while(true)
                    { // Normaler Ping Pong
                        int read = stream.Read(data, 0, data.Length);
                        string input = Encoding.ASCII.GetString(data, 0, read);

                        Console.WriteLine($"Ping[{client.RemoteEndPoint}]: {input}");
                        stream.Write(Encoding.ASCII.GetBytes(client.RemoteEndPoint.ToString()));
                    }
                }
            }
        }

        /// <summary>
        /// Verbindet sich zum Vermittlungsserver.<br/>
        /// Anschließend wird je nach Angabe vom Server, einen Hole-Punched-Server gestartet, oder zu einem Verbunden.<br/>
        /// Somit wird eine Peer-to-Peer Verbindung aufgebaut
        /// </summary>
        public static void ClientThread()
        {
            string endpointStr;
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            { // Verbindung mit Vermittlungsserver aufbauen
                client.Connect(IPEndPoint.Parse("127.0.0.1:51067"));

                using (NetworkStream stream = new NetworkStream(client))
                { // Speichere die IP-Adresse und Port vom externen hole punching Server
                    byte[] data = new byte[256];
                    int read = stream.Read(data, 0, data.Length);
                    endpointStr = Encoding.ASCII.GetString(data, 0, read);
                }

                // Starte hole punched Server, falls vom Server kein Endpunkt angegeben wurde...
                if (string.IsNullOrEmpty(endpointStr))
                    new Thread(PunchThread).Start(client.LocalEndPoint);
            }

            // Nur verbinden, wenn vom Server kein Endpunkt angegeben- und somit der hole punched Server nicht gestartet wurde...
            if (string.IsNullOrEmpty(endpointStr)) return;

            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            { // Baue eine Verbindung mit externen hole punched Server auf
                client.Connect(IPEndPoint.Parse(endpointStr));

                using(NetworkStream stream = new NetworkStream(client))
                { // Sende jede Sekunde eine Testnachricht
                    byte[] data = new byte[256];
                    while (true)
                    {
                        stream.Write(Encoding.ASCII.GetBytes(endpointStr));

                        int read = stream.Read(data, 0, data.Length);
                        string input = Encoding.ASCII.GetString(data, 0, read);
                        Console.WriteLine($"Pong[{client.RemoteEndPoint}]: {input}");

                        Thread.Sleep(1000);
                    }
                }
            }
        }

        /// <summary>
        /// Ein einfacher Vermittlungsserver der zwei clients befiehlt, sich miteinander zu verbinden
        /// </summary>
        public static void ServerThread()
        { // Starte Vermittlungsserver
            using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                server.Bind(IPEndPoint.Parse("127.0.0.1:51067"));
                server.Listen(2);

                using (Socket socket0 = server.Accept())
                using (Socket socket1 = server.Accept())
                using (NetworkStream stream0 = new NetworkStream(socket0))
                using (NetworkStream stream1 = new NetworkStream(socket1))
                {
                    IPEndPoint endpoint0 = (IPEndPoint)socket0.RemoteEndPoint;
                    IPEndPoint endpoint1 = (IPEndPoint)socket1.RemoteEndPoint;

                    // Eines von beiden kommentiert lassen!

                    //stream0.Write(Encoding.ASCII.GetBytes(endpoint1.ToString())); // Sende Endpoint von Peer1 zu Peer0
                    stream1.Write(Encoding.ASCII.GetBytes(endpoint0.ToString())); // Sende Endpoint von Peer0 zu Peer1
                }
            }

            Console.WriteLine("[Vermittlungsserver] Geschlossen!\n");
        }

    }
}
