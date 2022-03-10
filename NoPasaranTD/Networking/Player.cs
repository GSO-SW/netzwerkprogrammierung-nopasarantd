using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Networking
{
    public class Player
    {
        public string Name { get; set; }    
        public IPEndPoint EndPoint { get; set; }

        public Player(string info, IPEndPoint endPoint)
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
