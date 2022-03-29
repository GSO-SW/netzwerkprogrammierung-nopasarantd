using NoPasaranTD.Engine;
using NoPasaranTD.Networking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class LobbyListScreen : GuiComponent
    {

        private readonly ListContainer<NetworkLobby, LobbyItemContainer> lobbyList;
        private readonly TextBoxContainer txtNameContainer;
        private readonly ButtonContainer btnUpdatePlayer;
        private readonly ButtonContainer btnCreateLobby;

        private readonly GuiLobbyMenu parent;
        public LobbyListScreen(GuiLobbyMenu parent)
        {
            this.parent = parent;
            lobbyList = new ListContainer<NetworkLobby, LobbyItemContainer>()
            {
                Margin = 10,
                ItemSize = new Size((int)(StaticEngine.RenderWidth / 1.5f) - 20, 100),

                Position = new Point(StaticEngine.RenderWidth / 6, StaticEngine.RenderHeight / 6 - 20),
                ContainerSize = new Size((int)(StaticEngine.RenderWidth / 1.5f), (int)(StaticEngine.RenderHeight / 1.5f)),
                BackgroundColor = new SolidBrush(Color.FromArgb(225, Color.Gray)),

                Items = new NotifyCollection<NetworkLobby>()
            };
            lobbyList.SelectionChanged += JoinLobby;

            txtNameContainer = new TextBoxContainer
            {
                Margin = 2,
                Text = "MEEF" + (Environment.TickCount % 1337),
                Background = new SolidBrush(Color.White),
                Foreground = new SolidBrush(Color.Black),
                BorderBrush = new SolidBrush(Color.Black),
                Bounds = new Rectangle(5, 5, 230, 30),
                TextFont = StandartText2Font
            };

            // Aktualisiere Spielerinformationen
            btnUpdatePlayer = GuiLobbyMenu.CreateButton("Login", new Rectangle(240, 5, 100, 30));
            btnUpdatePlayer.ButtonClicked += () =>
            {
                if (IsNameValid(txtNameContainer.Text))
                {
                    UpdatePlayer(new NetworkClient(txtNameContainer.Text));
                }
            };

            // Erstelle neue Lobby
            btnCreateLobby = GuiLobbyMenu.CreateButton("Create Lobby", new Rectangle(
                lobbyList.Position.X + lobbyList.ContainerSize.Width - 150,
                lobbyList.Position.Y + lobbyList.ContainerSize.Height + 5,
                150, 30
            ));
            btnCreateLobby.ButtonClicked += () => CreateLobby(parent.LocalPlayer);
        }

        /// <summary>
        /// Aktualisiert die liste an Lobbies
        /// </summary>
        /// <param name="lobbies">Die neuen Lobbyinformationen</param>
        public void UpdateLobbies(List<NetworkLobby> lobbies)
        {
            lobbyList.Items.Clear();
            foreach (NetworkLobby lobby in lobbies)
            {
                lobbyList.Items.Add(lobby);
            }

            lobbyList.DefineItems();
        }

        #region Screen events
        private void UpdatePlayer(NetworkClient client)
        { // Befehl zum aktualisieren der Spielerinformation
            if (client == null || parent.DiscoveryClient == null)
            {
                return;
            }

            parent.LocalPlayer = client;
            if (parent.DiscoveryClient.LoggedIn)
            {
                parent.DiscoveryClient.UpdatePlayerAsync(client);
            }
            else
            {
                parent.DiscoveryClient.LoginAsync(client);
            }

            // Ursprünglich ist Content auf Login, da diese methode noch nicht ausgeführt wurde
            // Sobald der sich jedoch eingeloggt hat, soll ein anderer Content angezeigt werden
            btnUpdatePlayer.Content = "Update Info";
            txtNameContainer.Text = "";
        }

        private void CreateLobby(NetworkClient host)
        { // Befehl zum erstellen einer neuen lobby
            if (host == null || parent.DiscoveryClient == null || !parent.DiscoveryClient.LoggedIn)
            {
                return;
            }

            parent.DiscoveryClient.CreateLobbyAsync(new NetworkLobby(
                host, txtNameContainer.Text + "´s Room", "spentagon"
            ));
        }

        private void JoinLobby()
        { // Befehl zum Beitreten von Lobbies
            if (parent.DiscoveryClient == null || !parent.DiscoveryClient.LoggedIn)
            {
                return;
            }

            parent.DiscoveryClient.JoinLobbyAsync(lobbyList.SelectedItem);
        }
        #endregion

        #region Implementation region
        public override void Update()
        {
            lobbyList.Update();
            txtNameContainer.Update();
        }

        public override void Render(Graphics g)
        {
            txtNameContainer.Render(g);
            btnUpdatePlayer.Render(g);
            btnCreateLobby.Render(g);
            lobbyList.Render(g);

            if (parent.LocalPlayer != null)
            { // Render Login Zeichenkette
                string text = "Logged in as: " + parent.LocalPlayer.Name;
                Size size = TextRenderer.MeasureText(text, StandartText1Font);
                g.DrawString(text, StandartText1Font, Brushes.Black,
                    StaticEngine.RenderWidth - size.Width,
                    StaticEngine.RenderHeight - size.Height
                );
            }
        }

        public override void KeyUp(KeyEventArgs e)
        {
            lobbyList.KeyUp(e);
        }

        public override void KeyDown(KeyEventArgs e)
        {
            lobbyList.KeyDown(e);
            txtNameContainer.KeyDown(e);
        }

        public override void MouseUp(MouseEventArgs e)
        {
            lobbyList.MouseUp(e);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            txtNameContainer.MouseDown(e);
            btnUpdatePlayer.MouseDown(e);
            btnCreateLobby.MouseDown(e);
            lobbyList.MouseDown(e);
        }

        public override void KeyPress(KeyPressEventArgs e)
        {
            lobbyList.KeyPress(e);
            txtNameContainer.KeyPress(e);
        }

        public override void MouseMove(MouseEventArgs e)
        {
            lobbyList.MouseMove(e);
        }

        public override void MouseWheel(MouseEventArgs e)
        {
            lobbyList.MouseWheel(e);
        }
        #endregion

        private static bool IsNameValid(string text)
        {
            return !string.IsNullOrWhiteSpace(text)
                && !text.Contains("#") || !text.Contains("|");
        }
    }
}
