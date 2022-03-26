﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NoPasaranMS
{
    public class DiscoveryServer
    {
        private readonly int Port;
        private readonly Action<List<Player>> GroupFoundCallback;
        private readonly List<Lobby> Lobbies = new List<Lobby>();

        public DiscoveryServer(int port, Action<List<Player>> groupFoundCallback)
        {
            Port = port;
            GroupFoundCallback = groupFoundCallback;
            Lobbies.Add(new Lobby("FreePlayers"));
        }

        /// <summary>
        /// Startet den Vermittlungsserver.
        /// Hier wird nach Verbindungen gewartet und dann behandelt.
        /// </summary>
        public void Run()
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, Port);
            Socket serverSocket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endpoint);
            serverSocket.Listen(16);
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                new Thread(() => Receive(clientSocket)).Start();
            }
        }

        /// <summary>
        /// Behandelt eine bestimmte Tcp-Verbindung.
        /// Hier werden dann alle vom client gesendeten Befehle und seinen Verbindungsabbau behandelt
        /// </summary>
        /// <param name="clientSocket">Der Socket der zu verarbeiten ist</param>
        private void Receive(Socket clientSocket)
        {
            EndPoint endpoint = clientSocket.RemoteEndPoint;
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] new connection from {endpoint}");

            Player p = null;
            try
            {
                using (NetworkStream networkStream = new NetworkStream(clientSocket))
                using (StreamWriter writer = new StreamWriter(networkStream))
                using (StreamReader reader = new StreamReader(networkStream))
                {
                    // Warte auf einloggen des Spielers
                    string playerInfo = reader.ReadLine();
                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] added player {playerInfo}");
                    p = new Player(playerInfo, clientSocket, writer);
                    Lobbies[0].Players.Add(p);
                    SendUpdates(); // Teile jeden die Information des neuen Spielers mit

                    while (true)
                    {
                        string message = reader.ReadLine();
                        if (message == null)
                        {
                            throw new IOException("Stream was closed");
                        }

                        HandleMessage(p, message);
                    }
                }
            }
            catch (Exception)
            {
                if (p != null)
                {
                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] dropping {endpoint} '{p.Info}'");
                    RemovePlayerFromLobby(p);
                    SendUpdates();
                }
                else
                {
                    Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] dropping {endpoint}");
                }
            }
        }

        /// <summary>
        /// Behandelt die eingehende Nachricht eines Spielers.
        /// </summary>
        /// <param name="sender">Der Spieler der das Kommando gibt</param>
        /// <param name="message">Das Kommando selbst</param>
        private void HandleMessage(Player sender, string message)
        {
            try
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] handling message '{message}' from {sender.Info}");
                string type = message.Substring(0, message.IndexOf('#'));
                string content = message.Substring(message.IndexOf('#') + 1);
                Lobby lobby = Lobbies.Where(l => l.Players.Contains(sender)).FirstOrDefault();
                switch (type)
                {
                    case "SetUserInfo": // Aktualisiere die Spielerinformation
                        Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {sender.Info} send new user info '{content}'");
                        sender.Info = content;
                        break;
                    case "SetLobbyInfo": // Aktualisiere die Lobbyinformation
                        Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {sender.Info} send new lobby info for {lobby.Info} -> '{content}'");
                        lobby.Info = content;
                        break;
                    case "StartGame": // Starte das Spiel und führe das Event aus
                        Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {sender.Info} started game for lobby {lobby.Info}");
                        Lobbies.Remove(lobby);
                        GroupFoundCallback(lobby.Players.ToList());
                        break;
                    case "Join": // Füge den Spieler in die Lobby hinzu
                        Lobbies.Find(l => l.Info == content).Players.Add(sender);
                        RemovePlayerFromLobby(sender);
                        Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {sender.Info} joined {content}");
                        break;
                    case "Leave": // Entferne den Spieler von der Lobby
                        RemovePlayerFromLobby(sender);
                        Lobbies[0].Players.Add(sender);
                        Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {sender.Info} left {content}");
                        break;
                    case "NewLobby": // Erstelle eine neue Lobby
                        Lobbies.Add(new Lobby(content, sender));
                        RemovePlayerFromLobby(sender);
                        Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {sender.Info} created lobby {content}");
                        break;
                }
                SendUpdates(); // Teile die Änderung mit
            }
            catch (Exception)
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] weird message from {sender.Info}");
            }
        }

        /// <summary>
        /// Sendet an jede Verbindung den aktuellen Stand des Servers
        /// </summary>
        private void SendUpdates()
        {
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] sending updates -> {FullInfo()}");
            string info = FullInfo();
            for (int i = 0; i < Lobbies.Count; i++)
            {
                Lobby l = Lobbies[i];
                for (int j = 0; j < l.Players.Count; j++)
                {
                    Player p = l.Players[j];
                    try
                    {
                        p.Writer.WriteLine("Info#" + info);
                        p.Writer.Flush();
                    }
                    catch (Exception)
                    {
                        RemovePlayerFromLobby(p);
                    }
                }
            }
        }

        /// <summary>
        /// Entfernt einen Spieler von seiner aktuellen Lobby.
        /// Fügt ihn dann zu der Haupt-Lobby hinzu
        /// </summary>
        /// <param name="p"></param>
        private void RemovePlayerFromLobby(Player p)
        {
            int i = Lobbies.FindIndex(l => l.Players.Contains(p));
            if (i == -1)
            {
                return;
            }

            Lobbies[i].Players.Remove(p);
            if (i > 0 && Lobbies[i].Players.Count == 0)
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] removed lobby {Lobbies[i].Info}");
                Lobbies.RemoveAt(i);
            }
        }

        /// <summary>
        /// Erstelle einen Infostring des ganzen Stand des Vermittlungsservers
        /// </summary>
        /// <returns></returns>
        private string FullInfo()
        {
            return new StringBuilder().AppendJoin('\t', Lobbies.Select(l => l.FullInfo)).ToString();
        }
    }
}
