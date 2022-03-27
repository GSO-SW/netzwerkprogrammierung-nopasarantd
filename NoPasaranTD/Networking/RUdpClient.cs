using System;
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
        public byte Type { get; }
        public ulong Sequence { get; }
        public byte[] Data { get; }
        public RUdpPacket(byte type, ulong sequence, byte[] data)
        {
            Type = type;
            Sequence = sequence;
            Data = data;
        }

        public static byte[] Serialize(RUdpPacket packet)
        {
            byte[] data = new byte[17 + packet.Data.Length];

            data[0] = packet.Type;
            BitConverter.GetBytes(packet.Sequence).CopyTo(data, 1);
            BitConverter.GetBytes(packet.Data.LongLength).CopyTo(data, 9);
            Array.Copy(packet.Data, 0, data, 17, packet.Data.LongLength);
            return data;
        }

        public static RUdpPacket Deserialize(byte[] data)
        {
            byte type = data[0];
            ulong id = BitConverter.ToUInt64(data, 1);
            long length = BitConverter.ToInt64(data, 9);

            byte[] packet = new byte[length];
            Array.Copy(data, 17, packet, 0, length);
            return new RUdpPacket(type, id, packet);
        }

    }

    internal struct RUdpPacketCombo
    {
        public RUdpPacket Packet { get; }
        public IPEndPoint Endpoint { get; }
        public RUdpPacketCombo(RUdpPacket packet, IPEndPoint endpoint)
        {
            Packet = packet;
            Endpoint = endpoint;
        }
    }

    internal class RUdpPacketInfo
    {
        public int TickCreated { get; set; }
        public int TickSent { get; set; }

        public RUdpPacket Packet { get; }
        public List<IPEndPoint> Endpoints { get; } 
        public RUdpPacketInfo(RUdpPacket packet, IPEndPoint[] endpoints)
        {
            Packet = packet;
            Endpoints = new List<IPEndPoint>(endpoints);
            TickCreated = Environment.TickCount;
            TickSent = Environment.TickCount;
        }
    }

    internal class RUdpClientInfo
    {
        public uint Ping { get; set; }
        public ulong SequenceID { get; set; }

        public RUdpClientInfo()
        {
            Ping = 0;
            SequenceID = 0;
        }
    }

    public class RUdpClient : IDisposable
    {

        internal const byte CODE_PKG = 0x01;
        internal const byte CODE_ACK = 0x02;
        internal const byte CODE_SYN = 0x03;

        private readonly RUdpClientInfo localClient;
        private readonly Dictionary<IPEndPoint, RUdpClientInfo> remoteClients;

        private readonly Dictionary<ulong, RUdpPacketInfo> packetsSent;
        private readonly Queue<RUdpPacketCombo> packetsReceived;

        private readonly UdpClient udpClient;
        public RUdpClient(UdpClient client)
        {
            udpClient = client;
            localClient = new RUdpClientInfo();
            remoteClients = new Dictionary<IPEndPoint, RUdpClientInfo>();

            packetsSent = new Dictionary<ulong, RUdpPacketInfo>();
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

        public async Task SendAsync(byte[] data, params IPEndPoint[] endpoints)
        {
            RUdpPacket packet = new RUdpPacket(CODE_PKG, localClient.SequenceID, data);
            packetsSent[packet.Sequence] = new RUdpPacketInfo(packet, endpoints);

            foreach (IPEndPoint endpoint in endpoints)
                await SendPacketAsync(packet, endpoint);

            localClient.SequenceID++;
        }

        public async Task<UdpReceiveResult> ReceiveAsync()
        {
            while (packetsReceived.Count == 0) await Task.Delay(1);
            RUdpPacketCombo combo = packetsReceived.Dequeue();
            return new UdpReceiveResult(combo.Packet.Data, combo.Endpoint);
        }

        #region Implementation region
        private async void BackgroundThread()
        {
            while (true)
            {
                int tick = Environment.TickCount;
                for (int i = packetsSent.Count - 1; i >= 0; i--)
                {
                    RUdpPacketInfo info = packetsSent.Values.ElementAt(i);
                    for (int j = info.Endpoints.Count - 1; j >= 0; j--)
                    {
                        RUdpClientInfo client = GetRemoteClient(info.Endpoints[j]);
                        if (tick - info.TickSent >= Math.Max(100, client.Ping))
                        {
                            await SendPacketAsync(info.Packet, info.Endpoints[j]);
                            info.TickSent = Environment.TickCount;
                        }
                    }
                }

                while (udpClient.Available > 0)
                {
                    RUdpPacketCombo combo = await ReceivePacketAsync();
                    switch (combo.Packet.Type)
                    {
                        case CODE_PKG: await AcceptPKG(combo); break;
                        case CODE_ACK: AcceptACK(combo); break;
                        case CODE_SYN: await AcceptSYN(combo); break;
                        default: throw new IOException("Invalid Packet received");
                    }
                }
            }
        }

        private async Task AcceptPKG(RUdpPacketCombo combo)
        {
            RUdpClientInfo client = GetRemoteClient(combo.Endpoint);
            if (combo.Packet.Sequence < client.SequenceID)
                return;
            else if(combo.Packet.Sequence > client.SequenceID)
            {
                await SendPacketAsync(new RUdpPacket(CODE_SYN, client.SequenceID, new byte[0]), combo.Endpoint);
                return;
            }

            await SendPacketAsync(new RUdpPacket(CODE_ACK, client.SequenceID, new byte[0]), combo.Endpoint);
            packetsReceived.Enqueue(combo);
            client.SequenceID++;
        }

        private void AcceptACK(RUdpPacketCombo combo)
        {
            if (!packetsSent.TryGetValue(combo.Packet.Sequence, out RUdpPacketInfo info))
                throw new IOException("Packet was already acknowledged");

            if (!info.Endpoints.Remove(combo.Endpoint))
                throw new IOException("Packet was already acknowledged");

            RUdpClientInfo client = GetRemoteClient(combo.Endpoint);
            client.Ping = (uint)(Environment.TickCount - info.TickCreated);

            if(info.Endpoints.Count == 0)
            {
                if (!packetsSent.Remove(combo.Packet.Sequence))
                    throw new IOException("How tf did this happen now?");
            }
        }

        private async Task AcceptSYN(RUdpPacketCombo combo)
        {
            if(!packetsSent.TryGetValue(combo.Packet.Sequence, out RUdpPacketInfo info))
                throw new IOException("Packet was already acknowledged");

            if(!info.Endpoints.Contains(combo.Endpoint))
                throw new IOException("Packet was already acknowledged");

            await SendPacketAsync(info.Packet, combo.Endpoint);
        }
        #endregion

        #region Utility region
        private RUdpClientInfo GetRemoteClient(IPEndPoint endpoint)
        {
            if (!remoteClients.TryGetValue(endpoint, out RUdpClientInfo client))
                client = remoteClients[endpoint] = new RUdpClientInfo();
            return client;
        }

        private async Task SendPacketAsync(RUdpPacket packet, IPEndPoint endpoint)
        {
            byte[] data = RUdpPacket.Serialize(packet);
            await udpClient.SendAsync(data, data.Length, endpoint);
        }

        private async Task<RUdpPacketCombo> ReceivePacketAsync()
        {
            UdpReceiveResult result = await udpClient.ReceiveAsync();
            return new RUdpPacketCombo(
                RUdpPacket.Deserialize(result.Buffer),
                result.RemoteEndPoint
            );
        }
        #endregion

    }
}