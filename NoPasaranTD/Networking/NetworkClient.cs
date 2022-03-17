using System.Net;

namespace NoPasaranTD.Networking
{
    public class NetworkClient
    {
        public IPEndPoint EndPoint { get; internal set; }

        public string Name { get; set; }
        public NetworkClient(string name) => Name = name;

        /// <summary>
        /// Serialisiert einen NetworkClient zu einer Zeichenkette,
        /// die zum Vermittlungsserver gesendet werden kann
        /// </summary>
        /// <param name="client">Der zu serialisierende NetworkClient</param>
        /// <returns>Zeichenkette, die zum Vermittlungsserver gesendet werden kann</returns>
        public static string Serialize(NetworkClient client) => client.Name;

        /// <summary>
        /// Deserialisiert eine Zeichenkette zu einem NetworkClient,
        /// die vom Spiel ausgewertet werden kann.
        /// </summary>
        /// <param name="client">Die zu deserialisierende Zeichenkette</param>
        /// <returns>NetworkClient, der vom Spiel ausgewertet werden kann</returns>
        public static NetworkClient Deserialize(string info) => new NetworkClient(info);
    }
}
