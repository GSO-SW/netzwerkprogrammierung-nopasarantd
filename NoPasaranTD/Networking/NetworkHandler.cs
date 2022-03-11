using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace NoPasaranTD.Networking
{
    public class NetworkHandler : IDisposable
    {
        public UdpClient Socket { get; }
        public List<NetworkClient> Clients { get; }
        public Dictionary<string, Action<object>> EventHandlers { get; }

        public NetworkHandler(UdpClient socket, List<NetworkClient> clients)
        {
            Socket = socket;
            Clients = clients;
            EventHandlers = new Dictionary<string, Action<object>>();

            new Thread(ReceiveBroadcast).Start();                                
        }

        /// <summary>
        /// Versendet eine Nachricht an alle Lobbyteilnehmer
        /// </summary>
        /// <param name="message">Die Nachricht als String</param>
        public async void InvokeEvent(string command, object param)
        {
            string message = $"{command}({JsonConvert.SerializeObject(param, Formatting.None)})"; 
            byte[] encodedMessage = Encoding.UTF8.GetBytes(message);

            for (int i = 0; i < Clients.Count; i++)
                await Socket.SendAsync(encodedMessage, encodedMessage.Length, Clients[i].EndPoint);          
        }

        public void ReceiveBroadcast()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                while (true)
                {
                    
                    byte[] encodedMessage = Socket.Receive(ref endPoint);
                    string message = Encoding.UTF8.GetString(encodedMessage);

                    int firstIndex = message.IndexOf('(');
                    int lastIndex = message.LastIndexOf(')');

                    if (firstIndex == -1 || lastIndex == -1)
                    {
                        Console.WriteLine("Failed to parse message: " + message);                        
                        continue;
                    }

                    // COMMAND(PARAMETER)
                    string command = message.Substring(0, firstIndex);
                    string jsonString = message.Substring(firstIndex + 1, lastIndex - firstIndex - 1);

                    if(!EventHandlers.TryGetValue(command, out Action<object> handler))
                    {
                        Console.WriteLine("Cannot find such a command: " + command);
                        continue;
                    }

                    try { handler(JsonConvert.DeserializeObject(jsonString)); }                       
                    catch (Exception e) { Console.WriteLine("Cannot invoke handler: " + e.Message); }                                           
                }
            }
            catch { }
        }

        public void Dispose() =>
            Socket.Dispose();
    }         
}
