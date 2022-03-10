using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NoPasaranTD.Networking
{
    public class NetworkHandler
    {
        public UdpClient Socket { get; set; }
        public List<Player> Players { get; set; }

        public NetworkHandler(UdpClient socket, List<(string, IPEndPoint)> clients)
        {
            Socket = socket;
            foreach (var (userinfo, endpoint) in clients)
                Players.Add(new Player(userinfo, endpoint));
            
        }

        public async void SendBroadcast(string message)
        {

            for (int i = 0; i < Players.Count; i++)
            {
                // TODO: Nachricht Broadcast versenden
            }
            //await Socket.SendAsync()
        }

    }
         
}
