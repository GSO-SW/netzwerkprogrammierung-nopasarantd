﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using NoPasaranTD.Utilities;

namespace NoPasaranTD.Networking
{
    public class NetworkHandler : IDisposable
    {
        #region Eigenschaften

        /// <summary>
        /// Socket für die UDP-Protokoll Verbindung
        /// </summary>
        public UdpClient Socket { get; }

        /// <summary>
        /// Teilnehmer der derzeitigen Session
        /// </summary>
        public List<NetworkClient> Clients { get; }

        /// <summary>
        /// Alle möglichen Commands werden mit einer Methode hier abgelegt
        /// </summary>
        public Dictionary<string, Action<object>> EventHandlers { get; }

        /// <summary>
        /// Gibt an ob sich der NetworkHandler im Offlinemodus befindet
        /// </summary>
        public bool OfflineMode { get => Socket == null || Clients == null; }

        #endregion
        #region Konstruktor

        // Offlinemodus des Networkhandlers
        public NetworkHandler() => EventHandlers = new Dictionary<string, Action<object>>();

        // Onlinemodus des Networkhandlers
        public NetworkHandler(UdpClient socket, List<NetworkClient> clients)
        {
            Socket = socket;
            Clients = clients;
            EventHandlers = new Dictionary<string, Action<object>>();

            // Eröffnet einen neuen Thread für das Abhören neuer Nachrichten
            new Thread(ReceiveBroadcast).Start();
        }

        #endregion
        #region Methoden

        /// <summary>
        /// Versendet eine Nachricht an alle Lobbyteilnehmer
        /// </summary>
        /// <param name="message">Die Nachricht als String</param>
        public async void InvokeEvent(string command, object param)
        {
            if(!OfflineMode)
            {
                // Eine Nachricht wird erstellt mit folgendem Format:
                // "COMMAND"("PARAMETER")
                string message = $"{command}({Convert.ToBase64String(Serializer.SerializeObject(param))})";
                byte[] encodedMessage = Encoding.ASCII.GetBytes(message); // Die Nachricht wird zu einem Bytearray umgewandelt

                // Die Nachricht wird an alle Teilnehmer versendet
                for (int i = Clients.Count - 1; i >= 0; i--)
                {
                    try { await Socket.SendAsync(encodedMessage, encodedMessage.Length, Clients[i].EndPoint); } // Versuche nachricht an Empfänger zu Senden
                    catch (Exception ex) { Console.WriteLine(ex); Clients.RemoveAt(i); } // Gebe Fehlermeldung aus und lösche den Empfänger aus der Liste
                }
            }

            // Übergiebt die Methode die zum jeweiligen Command ausgeführt werden soll, wenn solch einer exisitiert
            if (!EventHandlers.TryGetValue(command, out Action<object> handler))
            {
                Console.WriteLine("Cannot find such a command: " + command);
                return;
            }

            // Führe event im client aus
            handler(param);
        }

        /// <summary>
        /// Methode für das Zuhören von Nachrichten in einer Session
        /// </summary>
        private void ReceiveBroadcast()
        {
            if (OfflineMode) throw new Exception("Can't receive input in OfflineMode");

            // Der IP-Endpunkt von dem Abgehört werden soll
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (true)
                {                
                    // Es wird nach einer Nachricht abgehört
                    byte[] encodedMessage = Socket.Receive(ref endPoint);
                    string message = Encoding.ASCII.GetString(encodedMessage);

                    // Index bei welchem der Parameter beginnt
                    int firstIndex = message.IndexOf('(');
                    // Index bei welchem der Parameter endet
                    int lastIndex = message.LastIndexOf(')');

                    if (firstIndex == -1 || lastIndex == -1) // Überprüft ob die Nachricht dem Format "COMMAND"("PARAMETER") entspricht
                    {
                        Console.WriteLine("Failed to parse message: " + message);                        
                        continue;
                    }

                    // COMMAND(PARAMETER)
                    string command = message.Substring(0, firstIndex); // Der Command der Nachricht
                    string base64String = message.Substring(firstIndex + 1, lastIndex - firstIndex - 1); // Die Weiteren Daten die übertragen wurden

                    // Übergiebt die Methode die zum jeweiligen Command ausgeführt werden soll, wenn solch einer exisitiert
                    if(!EventHandlers.TryGetValue(command, out Action<object> handler))
                    {
                        Console.WriteLine("Cannot find such a command: " + command);
                        continue;
                    }

                    try { handler(Serializer.DeserializeObject(Convert.FromBase64String(base64String))); } // Deserialisiert die Daten in ein Objekt                   
                    catch (Exception e) { Console.WriteLine("Cannot invoke handler: " + e.Message); }                                           
                }
            }
            catch(Exception e) 
            {
                Console.WriteLine(e);
            }
        }

        public void Dispose() => Socket?.Dispose();

        #endregion
    }
}
