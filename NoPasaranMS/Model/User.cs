using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace NoPasaranMS.Model
{
    public class User
    {
        public TcpClient TcpClient { get; set; }
        public string Username { get; set; }
    }
}
