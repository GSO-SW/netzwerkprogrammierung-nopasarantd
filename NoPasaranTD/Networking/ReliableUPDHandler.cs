using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Networking
{
    public class ReliableUPDHandler
    {
        private NetworkHandler networkHandler;

        /// <summary>
        /// Liste aller gesendeten Tasks auf die nicht alle Teilnehmer geantwortet haben
        /// </summary>
        private List<ReliableUDPModel> sendTasks = new List<ReliableUDPModel>();
        private List<NetworkTask> receivedTasks = new List<NetworkTask>();

        public ReliableUPDHandler(NetworkHandler ntwH)
        {
            networkHandler = ntwH;
            networkHandler.EventHandlers.Add("ReliableUDP", ReceiveReliableUDP);
            networkHandler.EventHandlers.Add("ReceiveAck", ReceiveAck);
            networkHandler.EventHandlers.Add("ResendTask", ReceiveResendReq);
        }

        /// <summary>
        /// Senden von RUDP
        /// </summary>
        public void SendReliableUDP(string command, object param)
        {
            sendTasks.Add(new ReliableUDPModel(new NetworkTask(command, param, networkHandler.Game.CurrentTick + networkHandler.HighestPing), networkHandler.Game.CurrentTick));
            networkHandler.InvokeEvent("ReliableUDP", sendTasks[sendTasks.Count - 1].NetworkTask, false);
        }

        /// <summary>
        /// Empfangen von RUDP
        /// </summary>
        public void ReceiveReliableUDP(object t)
        {
            //Console.WriteLine("Receive Task");
            foreach (var item in networkHandler.TaskQueue) // Checken, dass die Task nicht erneut gesendet wurde wegen eines Übertragungsfehlers an einen anderen Client
                if (item.ID == ((NetworkTask)t).ID)
                {
                    item.TickToPerform = ((NetworkTask)t).TickToPerform;
                    return;
                }
            foreach (var item in receivedTasks)
                if (item.ID == ((NetworkTask)t).ID)
                    return;
            networkHandler.TaskQueue.Add((NetworkTask)t); // Die Task in die Liste der zu erledigenden Aufgaben einfügen
            receivedTasks.Add((NetworkTask)t); // Die Task abspeichern, damit ein doppeltes Paket nach dem ausführen trotzdem noch erkannt wird
            if (!networkHandler.OfflineMode) // ACK Pakete müssen im Offlinemode an niemanden gesendet werden
                SendAck((NetworkTask)t);
        }

        /// <summary>
        /// Sende eine Antwort auf ein erhaltenes RUDP
        /// </summary>
        private void SendAck(NetworkTask networkTask)
        {
            if (networkTask.Hash == networkTask.GenerateHash()) // Die Checksum stimmt, also kann angenommen werden, dass die Übertragung erfolgreich war
                networkHandler.InvokeEvent("ReceiveAck", networkTask.ID, true);
            else // Die Checksum stimmt nicht, also gab es einen Übertragungsfehler und es wird ein neusenden angefragt
                networkHandler.InvokeEvent("ResendTask", networkTask.ID, true);
        }

        /// <summary>
        /// Wertet eine erhaltene Acknowledgement Nachricht aus
        /// </summary>
        public void ReceiveAck(object t)
        {
            for (int i = sendTasks.Count - 1; i >= 0; i--) // Alle noch nicht vollkommen abgeschlossenen Tasks durchgehen
            {
                if (sendTasks[i].NetworkTask.ID == Guid.Parse(t.ToString())) // Die beantwortete Task finden
                {
                    sendTasks[i].Answers.Add(true); // Abspeichern das es eine Antwort gab
                    if (sendTasks[i].Answers.Count == networkHandler.Participants.Count - 1) // Sollten alle Clients geantwortet haben, ausgeschlossen der sendende Client
                        sendTasks.RemoveAt(i); // Entfernen der Task, da alle Pakete erfolgreich angekommen sind
                }
            }
        }

        /// <summary>
        /// Empfangen einer Resendrequest falls die Übertragung nur teilweise erfolgreich war, mit einem Übertragungsfehler <br/>
        /// Sollte das angeforderte Paket nicht gefunden werden geschieht nichts
        /// </summary>
        public void ReceiveResendReq(object t)
        {
            for (int i = sendTasks.Count - 1; i >= 0; i--) // Alle gesendeten Pakete die nicht abgeschlossen sind durchgehen
                if (sendTasks[i].NetworkTask.ID == (Guid)t) // Kontrollieren, welches erneut gesendet werden soll
                    networkHandler.InvokeEvent("ReliableUDP", sendTasks[i].NetworkTask, true);
        }

        /// <summary>
        /// Kontrolliert, ob zu dem derzeitigen Zeitpunkt bereits eine Antwort auf die Pakete erwartet wird <br/>
        /// und wenn die Antworten überfällig sind, wird das Paket erneut gesendet.
        /// </summary>
        public void CheckPackageLifeTime()
        {
            for (int i = sendTasks.Count - 1; i >= 0; i--)
            {
                try // Es kann passieren, dass ein Packet entfernt wird, nachdem in die Forschleife gegangen wird aber bevor in die if-Kontrollstruktur gegangen wird
                {
                    // Ist der derzeitige Tick größer als die erwartete Zeit für eine Übertragung zum anderen Client und zurück
                    if (sendTasks[i].SendAtTick + (networkHandler.HighestPing / 2) < networkHandler.Game.CurrentTick)
                    {
                        sendTasks[i].SendAtTick = networkHandler.Game.CurrentTick; // Neues senden des Paketes abspeichern
                        sendTasks[i].NetworkTask.TickToPerform = networkHandler.Game.CurrentTick + networkHandler.HighestPing; // Neuen Tick zum ausführen festlegen, da dieser verschoben werden muss
                        networkHandler.InvokeEvent("ReliableUDP", sendTasks[i].NetworkTask, true); // In diesem Falle das Paket erneut senden
                    }
                        
                } 
                catch (Exception e)
                {
                    Console.WriteLine("Package deleted before reviewing: " + e.Message);
                }
            }

            for (int i = receivedTasks.Count - 1; i >= 0; i--) // Alle empfangenen Tasks durchgehen
                if (networkHandler.Game.CurrentTick - receivedTasks[i].TickToPerform > 10000) //schauen ob diese vor mehr als 10000 Ticks ausgeführt wurden, um den Speicherverbrauch und die Geschwindigkeit begrenzt zu halten
                    receivedTasks.RemoveAt(i);
        }
    }
}
