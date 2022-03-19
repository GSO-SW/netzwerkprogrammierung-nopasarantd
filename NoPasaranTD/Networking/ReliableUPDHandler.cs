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
        private List<ReliableUDPModell> sendTasks = new List<ReliableUDPModell>();

        public ReliableUPDHandler(NetworkHandler ntwH)
        {
            networkHandler = ntwH;
        }

        /// <summary>
        /// Senden von RUDP
        /// </summary>
        public void SendReliableUDP(string command, object param)
        {
            networkHandler.EventHandlers.TryGetValue(command, out Action<object> handler);
            sendTasks.Add(new ReliableUDPModell(new NetworkTask(handler, param, networkHandler.Game.CurrentTick + networkHandler.HighestPing), networkHandler.Game.CurrentTick));
            networkHandler.InvokeEvent("ReliableUDP", sendTasks[sendTasks.Count - 1].NetworkTask);
        }

        /// <summary>
        /// Empfangen von RUDP
        /// </summary>
        public void ReceiveReliableUDP(object t)
        {
            networkHandler.TaskQueue.Add((NetworkTask)t);
            SendAck(((NetworkTask)t).ID);
        }

        /// <summary>
        /// Sende eine Antwort auf ein erhaltenes RUDP
        /// </summary>
        private void SendAck(Guid id)
        {
            //TODO
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
                    sendTasks[i].Answers.Add(true);
                    if (sendTasks[i].Answers.Count == networkHandler.Participants.Count)
                        sendTasks.RemoveAt(i);
                }
            }
        }
    }
}
