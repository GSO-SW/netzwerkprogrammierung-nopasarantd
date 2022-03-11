using System.Net;

namespace NoPasaranTD.Networking
{
    public class NetworkClient
    {
        public IPEndPoint EndPoint { get; internal set; }

        public string Name { get; }
        public NetworkClient(string name) => Name = name;

        public static string Deserialize(NetworkClient client) => client.Name;
        public static NetworkClient Serialize(string info) => new NetworkClient(info);
    }
}
