using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NoPasaranTD.Networking
{
    internal struct RUdpPacket
    {

        public const byte CODE_UDP = 0x00;
        public const byte CODE_PKG = 0x01;
        public const byte CODE_ACK = 0x02;
        public const byte CODE_SYN = 0x03;

        public RUdpPacket(byte type, ulong sequence, byte[] data)
        {
            Type = type;
            Sequence = sequence;
            Data = data;
        }

        /// <summary>
        /// Pakettyp SYN, ACK oder PKG
        /// </summary>
        public byte Type { get; }

        /// <summary>
        /// Die Sequenz Nummer für dieses Paket
        /// </summary>
        public ulong Sequence { get; }

        /// <summary>
        /// Inhalt des Paketes
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Serialisiert das Paket zu einem byte[]
        /// </summary>
        public static byte[] Serialize(RUdpPacket packet)
        {
            if (packet.Type == CODE_UDP)
            {
                return packet.Data;
            }

            byte[] data = new byte[17 + packet.Data.Length];

            data[0] = packet.Type;
            BitConverter.GetBytes(packet.Sequence).CopyTo(data, 1);
            BitConverter.GetBytes(packet.Data.LongLength).CopyTo(data, 9);
            Array.Copy(packet.Data, 0, data, 17, packet.Data.LongLength);
            return data;
        }

        /// <summary>
        /// Deserialisiert den Datenstrom zu einem Paket
        /// </summary>
        public static RUdpPacket Deserialize(byte[] data)
        {
            byte type = data[0];
            ulong id = BitConverter.ToUInt64(data, 1);
            long length = BitConverter.ToInt64(data, 9);

            // UDP validität prüfen
            if (type < CODE_UDP || type > CODE_SYN || 17 + length != data.LongLength)
            {
                return new RUdpPacket(CODE_UDP, 0, data);
            }

            byte[] packet = new byte[length];
            Array.Copy(data, 17, packet, 0, length);
            return new RUdpPacket(type, id, packet);
        }

    }

    /// <summary>
    /// Kombination aus Paket und Endpunkt
    /// </summary>
    internal struct RUdpPacketCombo
    {
        public RUdpPacketCombo(RUdpPacket packet, IPEndPoint endpoint)
        {
            Packet = packet;
            Endpoint = endpoint;
        }

        public RUdpPacket Packet { get; }
        public IPEndPoint Endpoint { get; }
    }

    internal class RUdpPacketInfo
    {
        public RUdpPacketInfo(RUdpPacket packet, IPEndPoint[] endpoints)
        {
            Packet = packet;
            EndpointInfos = new ConcurrentDictionary<IPEndPoint, uint>();
            foreach (IPEndPoint endpoint in endpoints)
            {
                EndpointInfos[endpoint] = 0; // Anzahl von Verbindungsversuchen auf 0 initialisieren
            }

            TickCreated = Environment.TickCount;
            TickSent = Environment.TickCount;
        }

        /// <summary>
        /// Tick an dem dieses Paket erstellt wurde
        /// </summary>
        public int TickCreated { get; set; }

        /// <summary>
        /// Tick an dem dieses Paket zuletzt versendet wurde
        /// </summary>
        public int TickSent { get; set; }

        /// <summary>
        /// Paket was versendet werden soll
        /// </summary>
        public RUdpPacket Packet { get; }

        /// <summary>
        /// Endpunkte an dem das Paket noch zugestellt werden muss 
        /// und die zum senden dafür verbrauchten Versuche
        /// </summary>
        public ConcurrentDictionary<IPEndPoint, uint> EndpointInfos { get; }
    }

    internal class RUdpClientInfo
    {
        public RUdpClientInfo()
        {
            Ping = 0;
            SequenceID = 0;
        }

        /// <summary>
        /// Ping von diesem Benutzer
        /// </summary>
        public uint Ping { get; set; }

        /// <summary>
        /// Die Sequenz ID vom letzten versendeten Paket
        /// </summary>
        public ulong SequenceID { get; set; }
    }

    public class RUdpClient : IDisposable
    {

        /// <summary>
        /// Anzahl an Paketen die verloren gehen dürfen,
        /// bis der Endpunkt als nicht erreichbar gilt
        /// </summary>
        private const uint MAX_PACKET_LOSS = 15;

        /// <summary>
        /// Höchster Ping von allen noch verbundenen Endpunkten
        /// </summary>
        public uint HighestPing => remoteClients.Values.Select(c => c.Ping).Max();

        /// <summary>
        /// Event welches aufgerufen wird, sobald ein Client nicht mehr erreichbar ist
        /// </summary>
        public Action<IPEndPoint> OnRemoteTimeout;

        private readonly RUdpClientInfo localClient;
        private readonly ConcurrentDictionary<IPEndPoint, RUdpClientInfo> remoteClients;

        private readonly ConcurrentDictionary<ulong, RUdpPacketInfo> packetsSent;
        private readonly Queue<RUdpPacketCombo> packetsReceived;

        private readonly UdpClient udpClient;
        public RUdpClient(UdpClient client)
        {
            udpClient = client;
            localClient = new RUdpClientInfo();
            remoteClients = new ConcurrentDictionary<IPEndPoint, RUdpClientInfo>();

            packetsSent = new ConcurrentDictionary<ulong, RUdpPacketInfo>();
            packetsReceived = new Queue<RUdpPacketCombo>();
            new Thread(BackgroundThread).Start();
        }

        public void Dispose()
        {
            udpClient?.Dispose();
            remoteClients?.Clear();
            packetsReceived?.Clear();
            packetsSent?.Clear();
        }

        /// <summary>
        /// Sende über das normale UDP-Protokoll
        /// </summary>
        public async Task SendUnreliableAsync(byte[] data, params IPEndPoint[] endpoints)
        {
            foreach (IPEndPoint endpoint in endpoints)
            {
                await udpClient.SendAsync(data, data.Length, endpoint);
            }
        }

        /// <summary>
        /// Sende über das Reliable-UDP-Protokoll
        /// </summary>
        public async Task SendReliableAsync(byte[] data, params IPEndPoint[] endpoints)
        {
            RUdpPacket packet = new RUdpPacket(RUdpPacket.CODE_PKG, localClient.SequenceID, data);
            packetsSent[packet.Sequence] = new RUdpPacketInfo(packet, endpoints);

            foreach (IPEndPoint endpoint in endpoints)
            {
                await SendPacketAsync(packet, endpoint);
            }

            localClient.SequenceID++;
        }

        /// <summary>
        /// Erhalte ein UDP-Paket, sei es RUdp oder normales UDP
        /// </summary>
        public async Task<UdpReceiveResult> ReceiveAsync()
        {
            while (packetsReceived.Count == 0)
            {
                await Task.Delay(1);
            }

            RUdpPacketCombo combo = packetsReceived.Dequeue();
            return new UdpReceiveResult(combo.Packet.Data, combo.Endpoint);
        }

        #region Implementation region
        private async void BackgroundThread()
        {
            try
            {
                while (true)
                {
                    int tick = Environment.TickCount;
                    for (int i = packetsSent.Count - 1; i >= 0; i--)
                    {
                        RUdpPacketInfo info = packetsSent.Values.ElementAt(i);
                        for (int j = info.EndpointInfos.Count - 1; j >= 0; j--)
                        {
                            // Suche nach Endpunkt in diesem Index
                            IPEndPoint endpoint = info.EndpointInfos.Keys.ElementAt(j);
                            RUdpClientInfo client = GetRemoteClient(endpoint);

                            if (tick - info.TickSent >= Math.Max(100, client.Ping))
                            { // Sende das Paket erneut nach einer bestimmten Zeit
                                await SendPacketAsync(info.Packet, endpoint);
                                info.TickSent = Environment.TickCount;

                                info.EndpointInfos[endpoint]++; // Inkrementiere die anzahl an versuchen für diesen Endpunkt
                                if (info.EndpointInfos[endpoint] >= MAX_PACKET_LOSS)
                                {
                                    remoteClients.TryRemove(endpoint, out _); // Entferne Endpunkt
                                    RemovePacketOf(endpoint, info.Packet); // Entferne Paket aus dem Verlauf

                                    try
                                    {
                                        OnRemoteTimeout?.Invoke(endpoint);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                    }
                                }
                            }
                        }
                    }

                    while (udpClient.Available > 0)
                    {
                        RUdpPacketCombo combo = await ReceivePacketAsync();
                        switch (combo.Packet.Type)
                        {
                            case RUdpPacket.CODE_UDP: AcceptUDP(combo); break;
                            case RUdpPacket.CODE_PKG: await AcceptPKG(combo); break;
                            case RUdpPacket.CODE_ACK: AcceptACK(combo); break;
                            case RUdpPacket.CODE_SYN: await AcceptSYN(combo); break;
                            default: throw new IOException("Invalid Packet received");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void AcceptUDP(RUdpPacketCombo combo)
        {
            packetsReceived.Enqueue(combo);
        }

        private async Task AcceptPKG(RUdpPacketCombo combo)
        {
            RUdpClientInfo client = GetRemoteClient(combo.Endpoint);
            if (combo.Packet.Sequence < client.SequenceID)
            {
                return; // Server hinkt hinterher, ignoriere diese Nachricht
            }
            else if (combo.Packet.Sequence > client.SequenceID)
            { // Client hinkt hinterher, frage das Paket erneut an
                await SendPacketAsync(new RUdpPacket(RUdpPacket.CODE_SYN, client.SequenceID, new byte[0]), combo.Endpoint);
                return;
            }

            // Akzeptiere das Paket
            await SendPacketAsync(new RUdpPacket(RUdpPacket.CODE_ACK, client.SequenceID, new byte[0]), combo.Endpoint);
            packetsReceived.Enqueue(combo);
            client.SequenceID++;
        }

        private void AcceptACK(RUdpPacketCombo combo)
        {
            RUdpPacketInfo info = RemovePacketOf(combo.Endpoint, combo.Packet);

            RUdpClientInfo client = GetRemoteClient(combo.Endpoint);
            client.Ping = (uint)(Environment.TickCount - info.TickCreated);
        }

        private async Task AcceptSYN(RUdpPacketCombo combo)
        {
            if (!packetsSent.TryGetValue(combo.Packet.Sequence, out RUdpPacketInfo info))
            {
                throw new IOException("Packet was already acknowledged");
            }

            if (!info.EndpointInfos.ContainsKey(combo.Endpoint))
            {
                throw new IOException("Packet was already acknowledged");
            }

            // Versende das Paket erneut, wenn angefragt
            await SendPacketAsync(info.Packet, combo.Endpoint);
        }
        #endregion

        #region Utility region
        /// <summary>
        /// Entfernt das Paket welches zum Endpunkt gesendet wird, aus dem Verlauf.
        /// Gibt die Paketinformationen zurück, die aktualisiert wurden.
        /// </summary>
        private RUdpPacketInfo RemovePacketOf(IPEndPoint endpoint, RUdpPacket packet)
        {
            if (!packetsSent.TryGetValue(packet.Sequence, out RUdpPacketInfo info))
            {
                throw new IOException("Packet was already acknowledged");
            }

            // Entferne den Endpunkt aus der Liste
            if (!info.EndpointInfos.TryRemove(endpoint, out _))
            {
                throw new IOException("Packet was already acknowledged");
            }

            if (info.EndpointInfos.Count == 0)
            { // Wenn es niemanden mehr zum Informieren gibt, lösche das Paket aus dem Verlauf
                if (!packetsSent.TryRemove(packet.Sequence, out _))
                {
                    throw new IOException("How tf did this happen now?");
                }
            }

            return info;
        }

        private RUdpClientInfo GetRemoteClient(IPEndPoint endpoint)
        { // Suche nach dem Empfänger, wenn nicht gefunden, erstelle einen neuen
            if (!remoteClients.TryGetValue(endpoint, out RUdpClientInfo client))
            {
                client = remoteClients[endpoint] = new RUdpClientInfo();
            }

            return client;
        }

        private async Task SendPacketAsync(RUdpPacket packet, IPEndPoint endpoint)
        { // Serialisiere das Paket und versende es
            byte[] data = RUdpPacket.Serialize(packet);
            await udpClient.SendAsync(data, data.Length, endpoint);
        }

        private async Task<RUdpPacketCombo> ReceivePacketAsync()
        { // Nehme jegliches Paket an und deserialisiere es wieder
            UdpReceiveResult result = await udpClient.ReceiveAsync();
            return new RUdpPacketCombo(
                RUdpPacket.Deserialize(result.Buffer),
                result.RemoteEndPoint
            );
        }
        #endregion

    }
}