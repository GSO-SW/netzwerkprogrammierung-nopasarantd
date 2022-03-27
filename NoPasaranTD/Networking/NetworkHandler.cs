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
        /// <summary>
        /// Gibt zurück den letzten Zeitpunkt wo Update durchgeführt wurde auf Basis von Environment.TickCount()
        /// </summary>
        public int LastUpdate { get; private set; }

        public NetworkLobby Lobby { get; set; }

        #endregion
        #region Konstruktor

        private readonly List<NetworkTask> taskQueue;
        private readonly List<int> pings;
        private int highestPing = 0;

        // Offlinemodus des Networkhandlers
        public NetworkHandler()
        {
            EventHandlers = new Dictionary<string, Action<object>>();
            taskQueue = new List<NetworkTask>();
            pings = new List<int>();
        }

        // Onlinemodus des Networkhandlers
        public NetworkHandler(UdpClient socket, List<NetworkClient> participants, NetworkClient localPlayer)
        {
            Socket = socket;
            Participants = participants;
            LocalPlayer = localPlayer;

            EventHandlers = new Dictionary<string, Action<object>>();
            EventHandlers.Add("PingRequest", PingRequest);
            taskQueue = new List<NetworkTask>();
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
            LastUpdate = Environment.TickCount;
            for (int i = taskQueue.Count - 1; i >= 0; i--) // Alle Aufgaben in der Queue kontrollieren
            {
                if (taskQueue[i].Handler == PingRequest) // Checken, ob die Task ein PingRequest ist und direkt ausgeführt werden soll
                {
                    taskQueue[i].Handler(taskQueue[i].Parameter); // PingRequest ausführen
                    taskQueue.RemoveAt(taskQueue.Count - 1); // Task aus der Queue entfernen
                }
                else if (taskQueue[i].TickToPerform <= Game.CurrentTick  // Checken ob die Task bereits ausgeführt werden soll
                    || taskQueue[i].TickToPerform == 0)  // Wenn auf 0 gestellt dann ist es zum direkt ausführen
                {
                    if (taskQueue[i].TickToPerform == 0)
                        ;
                    taskQueue[i].Handler(taskQueue[i].Parameter); // Task ausführen
                    taskQueue.RemoveAt(taskQueue.Count - 1); // Task aus der Queue entfernen
                }
            }

            if (Game.CurrentTick % 500 == 0 && !OfflineMode)
                InvokeEvent("PingRequest", (long)Game.CurrentTick);
        }

        /// <summary>
        /// Versendet eine Nachricht an alle Lobbyteilnehmer
        /// </summary>
        /// <param name="instantExec">Ob bei ankommen auf den richtigen Tick gewartet werden soll oder dies vernachlässigt wird also direkt ausgeführt wird</param>
        /// <param name="message">Die Nachricht als String</param>
        public async void InvokeEvent(string command, object param, bool instantExec = false)
        {
            long tickToPerform = instantExec ? 0 : Game.CurrentTick + highestPing;

            if (!OfflineMode)
            {
                // Eine Nachricht wird erstellt mit folgendem Format:
                // "COMMAND"("PARAMETER")
                string message = $"{command}:{ tickToPerform }({Convert.ToBase64String(Serializer.SerializeObject(param))})";
                byte[] encodedMessage = Encoding.ASCII.GetBytes(message); // Die Nachricht wird zu einem Bytearray umgewandelt

                // Die Nachricht wird an alle Teilnehmer (außer einem selbst) versendet
                for (int i = Participants.Count - 1; i >= 0; i--)
                {
                    if (Participants[i].Equals(LocalPlayer)) continue;
                    try { await Socket.SendAsync(encodedMessage, encodedMessage.Length, Participants[i].EndPoint); } // Versuche nachricht an Empfänger zu Senden
                    catch (Exception ex) { Console.WriteLine(ex); Participants.RemoveAt(i); } // Gebe Fehlermeldung aus und lösche den Empfänger aus der Liste
                }
            }

            // Übergiebt die Methode die zum jeweiligen Command ausgeführt werden soll, wenn solch einer exisitiert
            if (!EventHandlers.TryGetValue(command, out Action<object> handler))
            {
                Console.WriteLine("Cannot find such a command: " + command);
                return;
            }

            // Führe event im client aus
            taskQueue.Add(new NetworkTask(handler, param, tickToPerform));
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

                    // Index bei welchem der Tick zum Einfügen beginnt
                    int firstIndexColon = message.IndexOf(':');
                    // Index bei welchem der Parameter beginnt
                    int firstIndexBracket = message.IndexOf('(');
                    // Index bei welchem der Parameter endet
                    int lastIndexBracket = message.LastIndexOf(')');

                    if (firstIndexColon == -1 || firstIndexBracket == -1 || lastIndexBracket == -1) // Überprüft ob die Nachricht dem Format "COMMAND"("PARAMETER") entspricht
                    {
                        Console.WriteLine("Failed to parse message: " + message);                        
                        continue;
                    }

                    // COMMAND(PARAMETER)
                    string command = message.Substring(0, firstIndexColon); // Der Command der Nachricht
                    string tickToPerform = message.Substring(firstIndexColon + 1, firstIndexBracket - firstIndexColon - 1); // Der Tick in dem das Ereignis ausgeführt werden soll
                    string base64String = message.Substring(firstIndexBracket + 1, lastIndexBracket - firstIndexBracket - 1); // Die Weiteren Daten die übertragen wurden

                    // Übergiebt die Methode die zum jeweiligen Command ausgeführt werden soll, wenn solch einer exisitiert
                    if(!EventHandlers.TryGetValue(command, out Action<object> handler))
                    {
                        Console.WriteLine("Cannot find such a command: " + command);
                        continue;
                    }

                    try { taskQueue.Add(new NetworkTask(handler, Serializer.DeserializeObject(Convert.FromBase64String(base64String)), Convert.ToInt64(tickToPerform))); } // Deserialisiert die Daten in ein Objekt                   
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
                highestPing = 0; // Ping erstmal wieder auf 0 als Basiswert setzen
                foreach (var item in pings) // Alle Eingegangenen Werte überprüfen
                    if (highestPing < item * 2) // Höchsten Ping suchen
                        highestPing = item * 2;
                pings.Clear();
            }
        }

        public void Dispose() => Socket?.Dispose();

        #endregion
    }
}
