using NoPasaranTD.Engine;
using NoPasaranTD.Networking;
using NoPasaranTD.Visuals.Ingame;
using System;
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
        GuiSelectMap gsm =new GuiSelectMap();
        private readonly StringFormat textFormat;
        private readonly ButtonContainer btnLeaveLobby;
        private readonly ButtonContainer btnStartGame;
        private readonly ButtonContainer btnNextMap;
        private readonly ButtonContainer btnPreviousMap;
        
        private readonly GuiMainMenu parent;
        public LobbyScreen(GuiMainMenu parent)
        {
            this.parent = parent;
            textFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center
            };

            gsm.GetMaps();
            
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
               StaticEngine.RenderWidth - StaticEngine.RenderWidth / 3 + 300,  StaticEngine.RenderHeight / 3 + 30, 100, 30
           ));
            btnNextMap.ButtonClicked += () =>
            {
                gsm.CurrentMap = ++gsm.CurrentMap % gsm.mapList.Count;
            };

            btnPreviousMap = GuiMainMenu.CreateButton("<", new Rectangle(
                StaticEngine.RenderWidth - StaticEngine.RenderWidth / 3 + 5, StaticEngine.RenderHeight / 3 + 30, 100, 30
            )); 
            
            btnPreviousMap.ButtonClicked += () =>
            {

                gsm.CurrentMap = Math.Abs(--gsm.CurrentMap % gsm.mapList.Count);
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
            Program.LoadGame(gsm.mapList.Values.ElementAt(gsm.CurrentMap));
        }
        #endregion

        #region Implementation region
        public override void Render(Graphics g)
        {
            btnLeaveLobby.Render(g);
            btnStartGame.Render(g);
            btnNextMap.Render(g);
            btnPreviousMap.Render(g);


            float scaledWidth = (float)StaticEngine.RenderWidth / gsm.mapList.Keys.ElementAt(gsm.CurrentMap).BackgroundImage.Width / 3;
            float scaledHeight = (float)StaticEngine.RenderHeight / gsm.mapList.Keys.ElementAt(gsm.CurrentMap).BackgroundImage.Height / 3;

            Matrix m = g.Transform;
            g.ScaleTransform(scaledWidth, scaledHeight);
            g.DrawImageUnscaled(gsm.mapList.Keys.ElementAt(gsm.CurrentMap).BackgroundImage, StaticEngine.RenderWidth- StaticEngine.RenderWidth/3, 0);
            g.Transform = m;

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
