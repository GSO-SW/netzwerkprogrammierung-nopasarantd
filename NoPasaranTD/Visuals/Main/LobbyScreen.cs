﻿using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using NoPasaranTD.Networking;
using NoPasaranTD.Utilities;
using NoPasaranTD.Visuals.Ingame;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class LobbyScreen : GuiComponent
    {

        /// <summary>
        /// Die Lobby die gerendert werden soll
        /// </summary>
        public NetworkLobby Lobby { get; set; }
        private readonly StringFormat textFormat;
        private readonly ButtonContainer btnLeaveLobby;
        private readonly ButtonContainer btnStartGame;
        private readonly ButtonContainer btnNextMap;
        private readonly ButtonContainer btnPreviousMap;
        private Dictionary<string,Map> mapList;
        private readonly GuiMainMenu parent;

        public LobbyScreen(GuiMainMenu parent)
        {
            mapList = Resources.LoadAllMaps();
            this.parent = parent;
            textFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center
            };

            btnLeaveLobby = GuiMainMenu.CreateButton("Leave Lobby", new Rectangle(
                5, StaticEngine.RenderHeight - 35,
                150, 30
            ));
            btnLeaveLobby.ButtonClicked += LeaveLobby;

            btnStartGame = GuiMainMenu.CreateButton("Start Game", new Rectangle(
                StaticEngine.RenderWidth - 155,
                StaticEngine.RenderHeight - 35,
                150, 30
            ));
            btnStartGame.ButtonClicked += StartGame;

            btnNextMap = GuiMainMenu.CreateButton(">", new Rectangle(
               StaticEngine.RenderWidth-105,  StaticEngine.RenderHeight / 3 + 5, 100, 30
           ));
            btnNextMap.ButtonClicked += () =>
            {
                int CurrentMap = Array.FindIndex(mapList.Keys.ToArray(), s => s.Equals(Lobby.MapName));
                CurrentMap = ++CurrentMap % mapList.Count;
                Lobby.MapName = mapList.Keys.ElementAt(CurrentMap);
                parent.DiscoveryClient.UpdateLobbyAsync(Lobby);

            };

            btnPreviousMap = GuiMainMenu.CreateButton("<", new Rectangle(
                StaticEngine.RenderWidth - StaticEngine.RenderWidth / 3+5 , StaticEngine.RenderHeight / 3 + 5, 100, 30
            )); 
            
            btnPreviousMap.ButtonClicked += () =>
            {
                int CurrentMap = Array.FindIndex(mapList.Keys.ToArray(),s=>s.Equals(Lobby.MapName));
                CurrentMap = Math.Abs(--CurrentMap % mapList.Count);
                Lobby.MapName = mapList.Keys.ElementAt(CurrentMap);
                parent.DiscoveryClient.UpdateLobbyAsync(Lobby);
            };


        }

        #region Event region
        private void LeaveLobby()
        { // Befehl zum Verlassen der Lobby
            if (parent.DiscoveryClient == null || !parent.DiscoveryClient.LoggedIn) return;
            parent.DiscoveryClient.LeaveCurrentLobbyAsync();
        }

        private void StartGame()
        { // Befehl zum Starten des Spiels
            if (parent.DiscoveryClient == null || !parent.DiscoveryClient.LoggedIn) return;
            parent.DiscoveryClient.StartGameAsync();
            Program.LoadGame(Lobby.MapName);
        }
        #endregion

        #region Implementation region

        public override void Render(Graphics g)
        {
            btnLeaveLobby.Render(g);
            btnStartGame.Render(g);
            btnNextMap.Render(g);
            btnPreviousMap.Render(g);


            
            g.DrawImage(mapList[Lobby.MapName].BackgroundImage, StaticEngine.RenderWidth- StaticEngine.RenderWidth/3, 0,StaticEngine.RenderWidth/3,StaticEngine.RenderHeight/3);

            // Lobby name
            g.DrawString(Lobby.Name, StandartHeader1Font, Brushes.Black, 0, 0);

            // Players
            int y = (StaticEngine.RenderHeight - ((Lobby.Players.Count + 1) * 25)) / 2;
            g.DrawString("Players", StandartHeader2Font, Brushes.Black, StaticEngine.RenderWidth / 2, y, textFormat);
            y += 25;

            // Player list
            foreach (NetworkClient player in Lobby.Players)
            {
                string text = (player.Equals(Lobby.Host) ? "♔ " : "") + player.Name;
                g.DrawString(text, StandartText1Font, Brushes.Black, StaticEngine.RenderWidth / 2, y, textFormat);
                y += 25;
            }
        }

        public override void MouseDown(MouseEventArgs e)
        {
            btnLeaveLobby.MouseDown(e);
            btnStartGame.MouseDown(e);
            btnNextMap.MouseDown(e);
            btnPreviousMap.MouseDown(e);
        }
        #endregion

    }
}
