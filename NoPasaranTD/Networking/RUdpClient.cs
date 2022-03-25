using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NoPasaranTD.Networking
{
    struct RUdpPacket
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
            Binary.WriteBytes(packet.Sequence, data, 1);
            Binary.WriteBytes(packet.Data.LongLength, data, 9);
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

    struct RUdpReceiveResult
    {
        public RUdpPacket Packet { get; }
        public IPEndPoint Endpoint { get; }
        public RUdpReceiveResult(RUdpPacket packet, IPEndPoint ep)
        {
            Packet = packet;
            Endpoint = ep;
        }
    }

    public class RUdpClient : IDisposable
    {

        private const byte CODE_PKG = 0x00;
        private const byte CODE_ACK = 0x01;
        private const byte CODE_SYN = 0x02;

        public int Ping { get; private set; } = 0;

        private ulong sequence = 0;

        private readonly UdpClient udpClient;
        private readonly Stopwatch pingStopwatch;
        private readonly Dictionary<ulong, RUdpPacket> packetsSent;
        private readonly Queue<RUdpReceiveResult> packetsReceived;
        public RUdpClient(UdpClient client)
        {
            udpClient = client;
            pingStopwatch = new Stopwatch();
            packetsSent = new Dictionary<ulong, RUdpPacket>();
            packetsReceived = new Queue<RUdpReceiveResult>();
        }

        public void Dispose()
        {
            udpClient?.Dispose();
            packetsReceived?.Clear();
            packetsSent?.Clear();
        }

        private async Task SendPacketReliableAsync(RUdpPacket packet, IPEndPoint ep)
        {
            await SendPacketAsync(packet, ep);

            while(true)
            {
                // Warte bis eine Nachricht angekommen ist oder die zeit abgelaufen ist
                int currTick = Environment.TickCount;
                while (udpClient.Available == 0 && (Environment.TickCount - currTick) < Ping)
                    await Task.Delay(1);

                if(udpClient.Available > 0)
                {
                    RUdpReceiveResult result = await ReceivePacketAsync();
                    switch(result.Packet.Type)
                    {
                        case CODE_PKG: // Add to receive queue
                            packetsReceived.Enqueue(result);
                            break;
                        case CODE_ACK: // Mark as acknowledged
                            if (!packetsSent.Remove(result.Packet.Sequence))
                                throw new IOException("Received an invalid Packet");
                            break;
                        case CODE_SYN: // Resend
                            if (!packetsSent.TryGetValue(result.Packet.Sequence, out RUdpPacket cachedPacket))
                                throw new IOException("Received an invalid Packet");
                            await SendPacketReliableAsync(cachedPacket, result.Endpoint);
                            break;
                        default: throw new IOException("Received an invalid Packet");
                    }

                    if (!packetsSent.ContainsKey(result.Packet.Sequence))
                        break;
                }
                else
                {
                    await SendPacketReliableAsync(packet, ep);
                    break;
                }
            }
        }

        public async Task SendAsync(byte[] data, /*TODO: REMOVE*/ int length, IPEndPoint ep)
        {
            RUdpPacket packet = packetsSent[sequence] = new RUdpPacket(CODE_PKG, sequence, data);

            pingStopwatch.Restart();
            await SendPacketReliableAsync(packet, ep);
            pingStopwatch.Stop();

            Ping = (int)pingStopwatch.ElapsedMilliseconds;
            sequence++;
        }

        public async Task<UdpReceiveResult> ReceiveAsync()
        {
            RUdpReceiveResult result = packetsReceived.Count > 0 ?
                packetsReceived.Dequeue() : await ReceivePacketAsync();

            if (result.Packet.Type != CODE_PKG)
                throw new IOException("Received an invalid Packet");

            if (result.Packet.Sequence > sequence)
            {
                await SendPacketAsync(new RUdpPacket(CODE_SYN, sequence, new byte[0]), result.Endpoint);
                return await ReceiveAsync();
            }
            else if (result.Packet.Sequence < sequence)
                return await ReceiveAsync();

            await SendPacketAsync(new RUdpPacket(CODE_ACK, sequence, new byte[0]), result.Endpoint);
            sequence++; // Inkrementiere die Sequenznummer

            return new UdpReceiveResult(result.Packet.Data, result.Endpoint);
        }

        #region Utility methods
        private async Task SendPacketAsync(RUdpPacket packet, IPEndPoint ep)
        {
            byte[] data = RUdpPacket.Serialize(packet);
            await udpClient.SendAsync(data, data.Length, ep);
        }

        private async Task<RUdpReceiveResult> ReceivePacketAsync()
        {
            UdpReceiveResult result = await udpClient.ReceiveAsync();
            return new RUdpReceiveResult(
                RUdpPacket.Deserialize(result.Buffer),
                result.RemoteEndPoint
            );
        }
        #endregion

    }
}
