using NoPasaranTD.Engine;
using NoPasaranTD.Networking;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class LobbyScreen : GuiComponent
    {

        public NetworkLobby Lobby { get; set; }

        private readonly StringFormat textFormat;
        private readonly ButtonContainer btnLeaveLobby;
        private readonly ButtonContainer btnStartGame;

        private readonly GuiMainMenu parent;
        public LobbyScreen(GuiMainMenu parent)
        {
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
        }

        #region Event region
        private void LeaveLobby()
        {
            if (parent.DiscoveryClient == null || !parent.DiscoveryClient.LoggedIn) return;
            parent.DiscoveryClient.LeaveCurrentLobbyAsync();
        }

        private void StartGame()
        {
            if (parent.DiscoveryClient == null || !parent.DiscoveryClient.LoggedIn) return;
            parent.DiscoveryClient.StartGameAsync();
        }
        #endregion

        #region Implementation region
        public override void Render(Graphics g)
        {
            btnLeaveLobby.Render(g);
            btnStartGame.Render(g);

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
        }
        #endregion

    }
}
