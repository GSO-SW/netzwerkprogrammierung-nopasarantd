using System;

namespace NoPasaranTD.Networking
{
    [Serializable]
    public class NetworkTask
    {
        public NetworkTask(Action<object> handler, object parameter, long tickToPerform)
        {
            Handler = handler;
            Parameter = parameter;
            TickToPerform = tickToPerform;
            ID = Guid.NewGuid();
        }

        /// <summary>
        /// Handler der ausgeführt werden soll
        /// </summary>
        public Action<object> Handler { get; }

        /// <summary>
        /// Parameter der dem Handler übergeben werden soll
        /// </summary>
        public object Parameter { get; }

        /// <summary>
        /// Zeitpunkt in Ticks an dem der Handler ausgeführt werden soll
        /// </summary>
        public long TickToPerform { get; }

        /// <summary>
        /// Id der Task um sie zuordnen zu können
        /// </summary>
        public Guid ID { get; }
    }
}
