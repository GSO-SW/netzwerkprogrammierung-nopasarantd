using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using NoPasaranTD.Networking;
using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        private readonly Dictionary<string, Map> mapList;

        private readonly GuiMainMenu parent;
        public LobbyScreen(GuiMainMenu parent)
        {
            this.parent = parent;
            mapList = ResourceLoader.LoadAllMaps();

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

            btnPreviousMap = GuiMainMenu.CreateButton("<", new Rectangle(
                StaticEngine.RenderWidth - StaticEngine.RenderWidth / 3 + 5, StaticEngine.RenderHeight / 3 + 5, 100, 30
            ));

            btnPreviousMap.ButtonClicked += () =>
            {
                int currentIndex = Array.FindIndex(mapList.Keys.ToArray(), s => s.Equals(Lobby.MapName));
                currentIndex = Math.Abs(--currentIndex % mapList.Count);

                Lobby.MapName = mapList.Keys.ElementAt(currentIndex);
                parent.DiscoveryClient.UpdateLobbyAsync(Lobby);
            };

            btnNextMap = GuiMainMenu.CreateButton(">", new Rectangle(
               StaticEngine.RenderWidth - 105, StaticEngine.RenderHeight / 3 + 5, 100, 30
            ));

            btnNextMap.ButtonClicked += () =>
            {
                int currentIndex = Array.FindIndex(mapList.Keys.ToArray(), s => s.Equals(Lobby.MapName));
                currentIndex = ++currentIndex % mapList.Count;

                Lobby.MapName = mapList.Keys.ElementAt(currentIndex);
                parent.DiscoveryClient.UpdateLobbyAsync(Lobby);
            };
        }

        public override void Dispose()
        {
            foreach (Map map in mapList?.Values)
                map.Dispose();
            mapList?.Clear();
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
        }
        #endregion

        #region Implementation region
        public override void Render(Graphics g)
        {
            btnLeaveLobby.Render(g);
            btnStartGame.Render(g);
            btnPreviousMap.Render(g);
            btnNextMap.Render(g);

            // Map preview
            g.DrawImage(mapList[Lobby.MapName].BackgroundImage, 
                StaticEngine.RenderWidth - StaticEngine.RenderWidth / 3, 0, 
                StaticEngine.RenderWidth / 3, StaticEngine.RenderHeight / 3
            );

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
            btnPreviousMap.MouseDown(e);
            btnNextMap.MouseDown(e);
        }
        #endregion

    }
}
