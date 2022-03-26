using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using NoPasaranTD.Utilities;
using NoPasaranTD.Engine;
using NoPasaranTD.Model;

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

        public bool Resyncing { get; private set; } = false;

        public NetworkLobby Lobby { get; set; }

        public ReliableUPDHandler ReliableUPD { get; }

        #endregion
        #region Konstruktor

        public readonly List<NetworkTask> TaskQueue;
        private readonly List<int> pings;
        public int HighestPing = 0;
        private List<NetworkTask> resyncPackageL = new List<NetworkTask>();

        // Offlinemodus des Networkhandlers
        public NetworkHandler()
        {
            EventHandlers = new Dictionary<string, Action<object>>();
            TaskQueue = new List<NetworkTask>();
            pings = new List<int>();
            ReliableUPD = new ReliableUPDHandler(this);
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
            EventHandlers.Add("ResyncReceive", ResyncReceive);
            EventHandlers.Add("ResyncReq", ResyncRequest);
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
                if (TaskQueue[i].Handler == "PingRequest")
                {
                    EventHandlers.TryGetValue(TaskQueue[i].Handler, out Action<object> handler);
                    handler(TaskQueue[i].Parameter); // Task ausführen
                    TaskQueue.RemoveAt(i);
                }
                else if (TaskQueue[i].TickToPerform == Game.CurrentTick 
                    || (TaskQueue[i].TickToPerform < Game.CurrentTick && TaskQueue[i].Handler == "ReliableUDP" && ((NetworkTask)TaskQueue[i].Parameter).Handler == "AddTower") 
                    || (TaskQueue[i].TickToPerform < Game.CurrentTick && TaskQueue[i].Handler == "AddTower")
                    || TaskQueue[i].Handler == "AddBalloon"
                    || OfflineMode) // Checken ob die Task ausgeführt werden soll
                {
                    Console.WriteLine(TaskQueue[i].Handler + "   " + TaskQueue[i].TickToPerform);
                    EventHandlers.TryGetValue(TaskQueue[i].Handler, out Action<object> handler);
                    handler(TaskQueue[i].Parameter); // Task ausführen
                    TaskQueue.RemoveAt(i); // Task aus der Queue entfernen
                }
                else if (TaskQueue[i].TickToPerform < Game.CurrentTick)
                {
                    Console.WriteLine("Desync detected  " + TaskQueue[i].Handler);
                    ReliableUPD.SendReliableUDP("ResyncReq",0); // TODO Item festlegen welches an den Host gesendet werden soll
                    TaskQueue.RemoveAt(i); // Task aus der Queue entfernen
                }
            }

            if (!OfflineMode)
                if (Game.CurrentTick % 500 == 0)
                    InvokeEvent("PingRequest", (long)Game.CurrentTick, false);
                

            ReliableUPD.CheckPackageLifeTime();
            
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
                TaskQueue.Add(new NetworkTask(command, param, Game.CurrentTick + 1));
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
                HighestPing = 100; // Ping erstmal wieder auf 0 als Basiswert setzen
                foreach (var item in pings) // Alle Eingegangenen Werte überprüfen
                    if (HighestPing < item * 4 * (int)StaticEngine.TickAcceleration) // Höchsten Ping suchen
                        HighestPing = item * 4 * (int)StaticEngine.TickAcceleration; // Die Verbindung muss im Zweifelsfall hin und her gehen, um ein Paket mit Sicherheit zu senden
                pings.Clear();
            }
        }

        /// <summary>
        /// Sendet alle entscheidenden Daten, wenn ein anderer Client desynchronisiert wurde, um wieder zu synchronisieren.
        /// Das senden geht nur vom Host aus.
        /// </summary>
        /// <param name="t"></param>
        private void ResyncRequest(object t)
        {
            if (IsHost) // Nur der Host soll Synchronisierungspakete senden
            {
                Resyncing = true;
                List<NetworkTask> tasks = new List<NetworkTask>();
                uint currentTick = Game.CurrentTick;
                tasks.Add(new NetworkTask("HPMoneyBlock", Game.HealthPoints, Game.Money)); // Übergibt die Leben und das Geld zur Zeit des zurücksetztens im Objekt und im TickToPerform
                foreach (var item in Game.Towers) // Fügt alle bereits platzierten Türme hinzu
                    tasks.Add(new NetworkTask("AddTower", item, currentTick));
                foreach (var item in Game.VTowers) // Fügt alle nur vorläufigen Türme hinzu
                    tasks.Add(new NetworkTask("AddTower", item, currentTick));
                for (int i = 0; i < Game.Balloons.Length; i++)
                    foreach (var item in Game.Balloons[i])
                        tasks.Add(new NetworkTask("AddBalloon", item, currentTick)); // Übergibt als TickToPerform den Pfadabschnitt in dem sich der Ballon befindet
                tasks.Insert(0, new NetworkTask("HEADER", tasks.Count, Game.CurrentTick)); // Erstellt den Header. Als Objekt wird die Anzahl aller Pakete, ausgeschlossen Header, übergeben. Als Tick den Tick auf den alles zurückgesetzt wird

                foreach (var item in tasks)
                    ReliableUPD.SendReliableUDP("ResyncReceive", item);
                Resyncing = false;
            }
        }

        /// <summary>
        /// Verwendet die Resync Nachrichten des Hosts um das Spiel wieder zu synchronisieren
        /// </summary>
        /// <param name="t"></param>
        private void ResyncReceive(object t)
        {
            resyncPackageL.Add((NetworkTask)t);
            if (((NetworkTask)t).Handler == "HEADER")
            {
                resyncPackageL.Insert(0, (NetworkTask)t); // Fügt den Header an die erste Stelle im Falle, dass die Pakete eine andere Reihenfolge haben
                resyncPackageL.RemoveAt(resyncPackageL.Count - 1);
            }
            if (resyncPackageL[0].Handler == "HEADER") // Kontrollieren, dass der Header angekommen ist
            {
                if ((int)resyncPackageL[0].Parameter == resyncPackageL.Count) // Kontrollieren, dass alle Pakete angekommen sind
                {
                    Resyncing = true; // Pausiert das Spiel, damit alle Aufgaben auf einmal passieren
                    Game.Towers.Clear(); // Entfernt alle Türme
                    Game.InitBalloons(); // Entfernt alle Ballons
                    Game.VTowers.Clear(); // Entfernt alle Türme die noch nicht vollkommen platziert sind
                    Game.CurrentTick = (uint)resyncPackageL[0].TickToPerform;
                    for (int i = resyncPackageL.Count - 1; i > 0; i--)
                    {
                        if (resyncPackageL[i].Handler == "HPMoneyBlock") // Sucht nach dem Paket in dem die Leben und das Geld übertragen werden
                        {
                            resyncPackageL.Insert(1, resyncPackageL[i]);
                            resyncPackageL.RemoveAt(i);
                            Game.HealthPoints = (int)resyncPackageL[1].Parameter; // Leben setzten
                            Game.Money = (int)resyncPackageL[1].TickToPerform; // Geld setzten
                            break;
                        }
                    }

                    for (int i = 2; i < resyncPackageL.Count; i++)
                        TaskQueue.Add(resyncPackageL[i]);
                    Update();
                    Game.CheckVTower();
                    Resyncing = false; // Führt das Spiel vom gesendeten Tick aus weiter
                }
            }
        }

        public void Dispose() => Socket?.Dispose();

        #endregion
    }
}
