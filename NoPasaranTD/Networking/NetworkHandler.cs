using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using NoPasaranTD.Utilities;
using NoPasaranTD.Engine;

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
        /// Socket für die UDP-Protokoll Verbindung
        /// </summary>
        public UdpClient Socket { get; }

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
        public bool OfflineMode { get => Socket == null || Participants == null; }

        /// <summary>
        /// Gibt an ob der ausgeführte Client der host der Sitzung ist
        /// </summary>
        public bool IsHost { get => OfflineMode || LocalPlayer == Participants[0]; }

        public ReliableUPDHandler ReliableUPD { get; }

        #endregion
        #region Konstruktor

        public readonly List<NetworkTask> TaskQueue;
        private readonly List<int> pings;
        public int HighestPing = 0;

        // Offlinemodus des Networkhandlers
        public NetworkHandler()
        {
            EventHandlers = new Dictionary<string, Action<object>>();
            TaskQueue = new List<NetworkTask>();
            pings = new List<int>();
            ReliableUPD = new ReliableUPDHandler(this);
            EventHandlers.Add("ReliableUDP", ReliableUPD.ReceiveReliableUDP);
            EventHandlers.Add("ReceiveAck", ReliableUPD.ReceiveAck);
        }

        // Onlinemodus des Networkhandlers
        public NetworkHandler(UdpClient socket, List<NetworkClient> participants, NetworkClient localPlayer)
        {
            Socket = socket;
            Participants = participants;
            LocalPlayer = localPlayer;

            EventHandlers = new Dictionary<string, Action<object>>();
            ReliableUPD = new ReliableUPDHandler(this);
            EventHandlers.Add("PingRequest", PingRequest);
            TaskQueue = new List<NetworkTask>();
            pings = new List<int>();

            // Eröffnet einen neuen Thread für das Abhören neuer Nachrichten
            new Thread(ReceiveBroadcast).Start();
        }

        #endregion
        #region Methoden

        /// <summary>
        /// Kontrolle, ob es Aufgaben gibt die in dem derzeitigen Tick erledigt werden müssen
        /// </summary>
        public void Update()
        {
            for (int i = TaskQueue.Count - 1; i >= 0; i--) // Alle Aufgaben in der Queue kontrollieren
            {
                if (TaskQueue[i].TickToPerform <= Game.CurrentTick) // Checken ob die Task bereits ausgeführt werden soll
                {
                    EventHandlers.TryGetValue(TaskQueue[i].Handler, out Action<object> handler);
                    handler(TaskQueue[i].Parameter); // Task ausführen
                    TaskQueue.RemoveAt(i); // Task aus der Queue entfernen
                }
            }

            if (!OfflineMode)
            {
                if (Game.CurrentTick % 500 == 0)
                    InvokeEvent("PingRequest", (long)Game.CurrentTick, false);
                ReliableUPD.CheckPackageLifeTime();
            }
            
        }

        /// <summary>
        /// Versendet eine Nachricht an alle Lobbyteilnehmer
        /// </summary>
        /// <param name="message">Die Nachricht als String</param>
        /// <param resend="resend">Soll das Paket selbst ausgeführt werden</param>
        public async void InvokeEvent(string command, object param, bool resend)
        {
            if(!OfflineMode)
            {
                // Eine Nachricht wird erstellt mit folgendem Format:
                // "COMMAND"("PARAMETER")
                string message = $"{command}({Convert.ToBase64String(Serializer.SerializeObject(param))})";
                byte[] encodedMessage = Encoding.ASCII.GetBytes(message); // Die Nachricht wird zu einem Bytearray umgewandelt

                // Die Nachricht wird an alle Teilnehmer (außer einem selbst) versendet
                for (int i = Participants.Count - 1; i >= 0; i--)
                {
                    if (Participants[i].Equals(LocalPlayer)) continue;
                    try { await Socket.SendAsync(encodedMessage, encodedMessage.Length, Participants[i].EndPoint); } // Versuche Nachricht an Empfänger zu Senden
                    catch (Exception ex) { Console.WriteLine(ex); Participants.RemoveAt(i); } // Gebe Fehlermeldung aus und lösche den Empfänger aus der Liste
                }
            }

            // Übergibt die Methode die zum jeweiligen Command ausgeführt werden soll, wenn solch einer existiert
            if (!EventHandlers.TryGetValue(command, out Action<object> handler))
            {
                Console.WriteLine("Cannot find such a command: " + command);
                return;
            }

            // Führe event im client aus
            if (!resend)
                TaskQueue.Add(new NetworkTask(command, param, -1));
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
                    int firstIndexBracket = message.IndexOf('(');
                    // Index bei welchem der Parameter endet
                    int lastIndexBracket = message.LastIndexOf(')');

                    if (firstIndexBracket == -1 || lastIndexBracket == -1) // Überprüft ob die Nachricht dem Format "COMMAND"("PARAMETER") entspricht
                    {
                        Console.WriteLine("Failed to parse message: " + message);                        
                        continue;
                    }

                    // COMMAND(PARAMETER)
                    string command = message.Substring(0, firstIndexBracket); // Der Command der Nachricht
                    string base64String = message.Substring(firstIndexBracket + 1, lastIndexBracket - firstIndexBracket - 1); // Die Weiteren Daten die übertragen wurden

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

        /// <summary>
        /// PingRequest Antworten vergleichen und den Höchsten Ping
        /// abspeichern, wenn dieser über 300 liegt
        /// </summary>
        /// <param name="t"></param>
        private void PingRequest(object t)
        {
            pings.Add((int)(Game.CurrentTick - (long)t)); // Delay zwischen senden 
            if (pings.Count == Participants.Count) // Nur kontrollieren, sobald alle Clients geantwortet haben
            {
                HighestPing = 0; // Ping erstmal wieder auf 0 als Basiswert setzen
                foreach (var item in pings) // Alle Eingegangenen Werte überprüfen
                    if (HighestPing < item * 4) // Höchsten Ping suchen
                        HighestPing = item * 4; // Die Verbindung muss im zweifelsfall 4 mal hin und her gehen, um ein Paket mit Sicherheit zu senden
                pings.Clear();
            }
        }

        public void Dispose() => Socket?.Dispose();

        #endregion
    }
}
