using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NoPasaranMS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var ds = new DiscoveryServer(int.Parse(args[0]), l => { });
            ds.Run();
        }
    }
}
