using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Networking
{
    [Serializable]
    public class ReliableUDPModel
    {
        public ReliableUDPModel(NetworkTask networkTask, long sendTick)
        {
            Answers = new List<bool>();
            NetworkTask = networkTask;
            SendAtTick = sendTick;
        }

        /// <summary>
        /// Die zu erledigende Task
        /// </summary>
        public NetworkTask NetworkTask { get; set; }

        /// <summary>
        /// Der Tick an dem das Paket gesendet wurde
        /// </summary>
        public long SendAtTick { get; set; }

        /// <summary>
        /// Ein BoolArray mit einem Eintrag für jeden verbundenen Mitspieler der auf false steht solange es keine positive Rückmeldung gegeben hat
        /// </summary>
        public List<bool> Answers { get; set; }
    }
}
