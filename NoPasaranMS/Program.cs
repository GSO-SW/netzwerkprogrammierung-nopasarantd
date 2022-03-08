namespace NoPasaranMS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int portTCP = args.Length >= 1 ? int.Parse(args[0]) : 31415;
            int portUDP = args.Length >= 2 ? int.Parse(args[0]) : (portTCP + 1);
            var ds = new DiscoveryServer(portTCP, l => UDPHolePunching.Connect(l, portUDP));
            ds.Run();
        }
    }
}
