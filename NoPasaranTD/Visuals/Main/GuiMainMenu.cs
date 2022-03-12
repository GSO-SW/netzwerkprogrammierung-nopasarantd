using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using NoPasaranTD.Networking;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class GuiMainMenu : GuiComponent
    {

        private static readonly SolidBrush BACKGROUND_COLOR = new SolidBrush(Color.FromArgb(75, Color.Black));

        private ListContainer<NetworkLobby, LobbyItemContainer> lobbyList;
        private ButtonContainer btnPlayLocalGame;
        private ButtonContainer btnCreateLobby;
        private Game backgroundGame;

        private string discoveryStatus;
        private DiscoveryClient discoveryClient;
        private NetworkClient localPlayer;
        private NetworkLobby currentLobby;

        public GuiMainMenu()
        {
            // Lade Spielszene
            Map map = MapData.GetMapByFileName("spentagon"); map.Initialize();
            backgroundGame = new Game(map, new NetworkHandler());
            {
                // UILayout unsichtbar und inaktiv schalten
                backgroundGame.UILayout.Active = false;
                backgroundGame.UILayout.Visible = false;

                // Spawne Türme in Spielszene
                Size towerSize = StaticInfo.GetTowerSize(typeof(TowerCanon));
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(520, 260), towerSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(855, 320), towerSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(590, 530), towerSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(160, 160), towerSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(740, 175), towerSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(225, 390), towerSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(460, 30), towerSize) });
            }
            
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

            // Erstelle neue Lobby
            btnCreateLobby = CreateButton("Create Lobby", new Rectangle(
                lobbyList.Position.X + lobbyList.ContainerSize.Width - 305,
                lobbyList.Position.Y + lobbyList.ContainerSize.Height + 5,
                150, 30
            ));
            btnCreateLobby.ButtonClicked += CreateLobby;

            // Lade privates Spiel
            btnPlayLocalGame = CreateButton("Play Local Game", new Rectangle(
                lobbyList.Position.X + lobbyList.ContainerSize.Width - 150,
                lobbyList.Position.Y + lobbyList.ContainerSize.Height + 5,
                150, 30
            ));
            btnPlayLocalGame.ButtonClicked += () => Program.LoadGame("spentagon");

            Task.Run(OpenDiscovery); // Initialisiere Verbindung mit Vermittlungsserver
        }

        private void OpenDiscovery()
        {
            try
            {
                discoveryStatus = "Connecting to server...";
                discoveryClient = new DiscoveryClient(Program.SERVER_ADDRESS, Program.SERVER_PORT)
                {
                    OnGameStart = StartGame,
                    OnInfoUpdate = UpdateInfo
                };
                discoveryStatus = "Login required!";

                // TODO: Implement properly
                UpdatePlayer(new NetworkClient("YEET"));
                discoveryStatus = null;
            }
            catch (Exception ex)
            {
                discoveryStatus = "Connection failed: " + ex.Message;
            }
        }

        private void UpdatePlayer(NetworkClient client)
        {
            if (discoveryClient == null) return;

            localPlayer = client;
            if (discoveryClient.LoggedIn)
                discoveryClient.UpdatePlayerAsync(client);
            else
                discoveryClient.LoginAsync(client);

            // Warte bis Spieler eingeloggt ist
            while (!discoveryClient.LoggedIn)
                Thread.Sleep(1);
        }

        #region Discovery event region
        private void CreateLobby()
        {
            if (discoveryClient == null || !discoveryClient.LoggedIn) return;

            // TODO: Change lobby name by textbox
            discoveryClient.CreateLobbyAsync(new NetworkLobby(localPlayer, "Lobby Name (By textbox)" + Environment.TickCount));
        }

        private void JoinLobby()
        {
            if (discoveryClient == null || !discoveryClient.LoggedIn) return;
            discoveryClient.JoinLobbyAsync(lobbyList.SelectedItem);
        }

        private void StartGame()
        {
            if (discoveryClient == null || !discoveryClient.LoggedIn) return;
            Program.LoadGame("spentagon", new NetworkHandler(
                discoveryClient.UdpClient, discoveryClient.Clients
            ));
        }

        private void UpdateInfo()
        {
            if (discoveryClient == null || !discoveryClient.LoggedIn) return;

            currentLobby = null;
            lobbyList.Items.Clear();
            foreach(NetworkLobby lobby in discoveryClient.Lobbies)
            {
                lobbyList.Items.Add(lobby);
                if (lobby.PlayerExists(localPlayer))
                    currentLobby = lobby;
            }
            lobbyList.DefineItems();

            // Setze sichtbarkeit von allen Lobby elementen
            // in abhängigkeit von der beigetretenen Lobby
            lobbyList.Visible = currentLobby == null;
            lobbyList.Active = currentLobby == null;
            btnPlayLocalGame.Visible = currentLobby == null;
            btnPlayLocalGame.Active = currentLobby == null;
            btnCreateLobby.Visible = currentLobby == null;
            btnCreateLobby.Active = currentLobby == null;
        }
        #endregion

        #region Implementation region
        public override void Update()
        {
            backgroundGame.Update();
            lobbyList.Update();
        }

        public override void Render(Graphics g)
        {
            // Spielszene rendern und dimmen
            backgroundGame.Render(g);
            g.FillRectangle(BACKGROUND_COLOR, 
                0, 0, StaticEngine.RenderWidth, StaticEngine.RenderHeight);

            btnPlayLocalGame.Render(g);
            btnCreateLobby.Render(g);
            lobbyList.Render(g);

            if(discoveryStatus != null)
            {
                Size textSize = TextRenderer.MeasureText(discoveryStatus, StandartText1Font);
                g.DrawString(discoveryStatus, StandartText1Font, Brushes.Red, 0, StaticEngine.RenderHeight - textSize.Height);
            }
        }

        public override void KeyUp(KeyEventArgs e)
        {
            backgroundGame.KeyUp(e);
            lobbyList.KeyUp(e);
        }

        public override void KeyDown(KeyEventArgs e)
        {
            backgroundGame.KeyDown(e);
            lobbyList.KeyDown(e);
        }

        public override void MouseUp(MouseEventArgs e)
        {
            backgroundGame.MouseUp(e);
            lobbyList.MouseUp(e);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            backgroundGame.MouseDown(e);
            btnPlayLocalGame.MouseDown(e);
            btnCreateLobby.MouseDown(e);
            lobbyList.MouseDown(e);
        }

        public override void MouseMove(MouseEventArgs e)
        {
            backgroundGame.MouseMove(e);
            lobbyList.MouseMove(e);
        }

        public override void MouseWheel(MouseEventArgs e)
        {
            backgroundGame.MouseWheel(e);
            lobbyList.MouseWheel(e);
        }
        #endregion

        private static ButtonContainer CreateButton(string text, Rectangle bounds)
        {
            return new ButtonContainer
            {
                Bounds = bounds,
                Content = text,
                StringFont = StandartText1Font,
                Foreground = Brushes.Black,
                Background = Brushes.LightGray,
                BorderBrush = Brushes.Blue,
                Margin = 2
            };
        }

    }
}
