using NoPasaranTD.Engine;
using NoPasaranTD.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NoPasaranTD.Networking
{
    public class NetworkHandler : IDisposable
    {
        #region Eigenschaften

        /// <summary>
        /// Game instanz auf die dieser Handler basiert
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// Socket für die ReliableUDP-Protokoll Verbindung
        /// </summary>
        public RUdpClient Socket { get; }

        /// <summary>
        /// Teilnehmer der derzeitigen Session.<br/>
        /// Ist eine Null-Referenz im OfflineModus (Keine Spieler vorhanden)
        /// </summary>
        public List<NetworkClient> Participants { get; }

        /// <summary>
        /// Der lokale Spieler der Session.<br/>
        /// Ist eine Null-Referenz im OfflineModus (Kein lokaler Spieler vorhanden)
        /// </summary>
        public NetworkClient LocalPlayer { get; }

        /// <summary>
        /// Alle möglichen Commands werden mit einer Methode hier abgelegt
        /// </summary>
        public Dictionary<string, Action<object>> EventHandlers { get; }

        /// <summary>
        /// Gibt an ob sich der NetworkHandler im Offlinemodus befindet
        /// </summary>
        public bool OfflineMode => Socket == null || Participants == null;

        /// <summary>
        /// Gibt an ob der ausgeführte Client der host der Sitzung ist
        /// </summary>
        public bool IsHost => OfflineMode || LocalPlayer == Participants[0];

        public NetworkLobby Lobby { get; set; }

        #endregion
        #region Konstruktor

        // Offlinemodus des Networkhandlers
        public NetworkHandler()
        {
            EventHandlers = new Dictionary<string, Action<object>>();
        }

        // Onlinemodus des Networkhandlers
        public NetworkHandler(RUdpClient socket, List<NetworkClient> participants, NetworkClient localPlayer)
        {
            Socket = socket;
            Participants = participants;
            LocalPlayer = localPlayer;

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
            if (!OfflineMode)
            {
                // Eine Nachricht wird erstellt mit folgendem Format: "COMMAND"("PARAMETER")
                string message = $"{command}({Convert.ToBase64String(Serializer.SerializeObject(param))})";
                byte[] encodedMessage = Encoding.ASCII.GetBytes(message); // Die Nachricht wird zu einem Bytearray umgewandelt

                // Die Nachricht wird an alle Teilnehmer (außer einem selbst) versendet
                IPEndPoint[] endpoints = Participants.Where(p => !p.Equals(LocalPlayer)).Select(p => p.EndPoint).ToArray();
                await Socket.SendAsync(encodedMessage, endpoints);
            }

            // Übergibt die Methode die zum jeweiligen Command ausgeführt werden soll, wenn solch einer exisitiert
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
        private async void ReceiveBroadcast()
        {
            if (OfflineMode)
            {
                throw new Exception("Can't receive input in OfflineMode");
            }

            while (true)
            {
                // Es wird nach einer Nachricht abgehört
                byte[] encodedMessage = (await Socket.ReceiveAsync()).Buffer;
                string message = Encoding.ASCII.GetString(encodedMessage);

                // Index bei welchem der Parameter beginnt
                int firstIndexBracket = message.IndexOf('(');
                // Index bei welchem der Parameter endet
                int lastIndexBracket = message.LastIndexOf(')');

                if (firstIndexBracket == -1 || lastIndexBracket == -1) // Überprüft ob die Nachricht dem Format "COMMAND"("PARAMETER") entspricht
                {
                    Console.WriteLine("Failed to parse message: " + message);
                    continue;
                }

                // COMMAND(PARAMETER)
                string command = message.Substring(0, firstIndexBracket);
                string base64String = message.Substring(firstIndexBracket + 1, lastIndexBracket - firstIndexBracket - 1);

                // Übergibt die Methode die zum jeweiligen Command ausgeführt werden soll, wenn solch einer exisitiert
                if (!EventHandlers.TryGetValue(command, out Action<object> handler))
                {
                    Console.WriteLine("Cannot find such a command: " + command);
                    continue;
                }
                    
                try { handler(Serializer.DeserializeObject(Convert.FromBase64String(base64String))); } // Deserialisiert die Daten in ein Objekt                   
                catch (Exception e) { Console.WriteLine("Cannot invoke handler: " + e.Message); }
            }
        }

        public void Dispose()
        {
            Socket?.Dispose();
        }

        #endregion
    }
}
