using System;
using System.Text;

namespace NoPasaranTD.Networking
{
    [Serializable]
    public class NetworkTask
    {
        public NetworkTask(string handler, object parameter, long tickToPerform)
        {
            Handler = handler;
            Parameter = parameter;
            TickToPerform = tickToPerform;
            ID = Guid.NewGuid();
            Hash = GenerateHash();
        }

        /// <summary>
        /// String des Handlers der ausgeführt werden soll
        /// </summary>
        public string Handler { get; }

        /// <summary>
        /// Parameter der dem Handler übergeben werden soll
        /// </summary>
        public object Parameter { get; }

        /// <summary>
        /// Zeitpunkt in Ticks an dem der Handler ausgeführt werden soll
        /// </summary>
        public long TickToPerform { get; set; }

        /// <summary>
        /// Id der Task um sie zuordnen zu können
        /// </summary>
        public Guid ID { get; }

        public string Hash { get; set; }

        /// <summary>
        /// Generiert einen Hashwert als Checksum für das Packet
        /// </summary>
        /// <returns></returns>
        public string GenerateHash()
        {
            string convert = $"{ID}.{Handler}.{Parameter}";
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                return BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(convert)));
        }
    }
}
