using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Networking
{
    public class NetworkClient
    {
        public string Name { get; private set; }    
        public IPEndPoint EndPoint { get; }

        public NetworkClient(string info, IPEndPoint endPoint)
        {
            EndPoint = endPoint;
            Parse(info);
        }

        private void Parse(string infoString)
        {
            Name = infoString;  
        }
    }
}
