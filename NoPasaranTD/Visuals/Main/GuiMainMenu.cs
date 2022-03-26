using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using NoPasaranTD.Networking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class GuiMainMenu : GuiComponent
    {

        private static readonly SolidBrush BACKGROUND_COLOR = new SolidBrush(Color.FromArgb(75, Color.Black));

        /// <summary>
        /// Der aktuelle Status zum Server.<br/>
        /// Ist eine null-Referenz wenn es keine Probleme gibt
        /// </summary>
        public string DiscoveryStatus { get; private set; }

        /// <summary>
        /// Instanz zum Kommunizieren mit dem Vermittlungsserver
        /// </summary>
        public DiscoveryClient DiscoveryClient { get; private set; }

        /// <summary>
        /// Die jetzig beigetretene Lobby
        /// </summary>
        public NetworkLobby CurrentLobby { get; private set; }

        /// <summary>
        /// Der Spieler als der man eingeloggt ist.<br/>
        /// Ist eine null-Referenz, wenn der Spieler nicht eingeloggt ist
        /// </summary>
        public NetworkClient LocalPlayer { get; set; }

        /// <summary>
        /// Der vom Menu angezeigte Screen
        /// </summary>
        public GuiComponent ForegroundScreen { get; set; }

        /// <summary>
        /// Screen zum Managen der Liste von Lobbies
        /// </summary>
        public LobbyListScreen LobbyListScreen { get; }

        /// <summary>
        /// Screen zum Managen einer einzelnen Lobby
        /// </summary>
        public LobbyScreen LobbyScreen { get; }

        private readonly Game backgroundGame;
        public GuiMainMenu()
        {
            // Lade Spielszene
            Map map = MapData.GetMapByFileName("spentagon"); map.Initialize();
            backgroundGame = new Game(map, new NetworkHandler());
            {
                // UILayout unsichtbar und inaktiv schalten
                backgroundGame.UILayout.Visible = false;
                backgroundGame.GodMode = true;

                // Spawne Türme in Spielszene
                Size towerSize = StaticInfo.GetTowerSize(typeof(TowerCanon));
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(520, 260), towerSize) }, false);
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(855, 320), towerSize) }, false);
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(590, 530), towerSize) }, false);
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(160, 160), towerSize) }, false);
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(740, 175), towerSize) }, false);
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(225, 390), towerSize) }, false);
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(460, 30), towerSize) }, false);
            }

            LobbyScreen = new LobbyScreen(this);
            LobbyListScreen = new LobbyListScreen(this);
            ForegroundScreen = LobbyListScreen;
            Task.Run(OpenDiscovery);
        }

        private void OpenDiscovery()
        { // Verbinde mit Vermittlungsserver
            try
            {
                DiscoveryStatus = "Connecting to server...";
                DiscoveryClient = new DiscoveryClient(Program.SERVER_ADDRESS, Program.SERVER_PORT)
                {
                    OnGameStart = StartGame,
                    OnInfoUpdate = UpdateInfo
                };
                DiscoveryStatus = "Login required!";
            }
            catch (Exception ex)
            {
                DiscoveryStatus = "Connection failed: " + ex.Message;
            }
        }

        public override void Dispose()
        {
            DiscoveryClient?.Dispose();
            backgroundGame?.Dispose();
        }

        #region Discovery event region
        private void StartGame()
        { // Befehl zum Starten des Spiels
            if (DiscoveryClient == null || !DiscoveryClient.LoggedIn) return;

            List<NetworkClient> participants = new List<NetworkClient>();
            { // Baue Mitspieler Liste auf (Host auf index 0)
                string playerStr;
                string hostStr = NetworkClient.Serialize(CurrentLobby.Host);
                foreach(NetworkClient player in DiscoveryClient.Clients)
                { // Alle Endpunkte (Außer lokaler Spieler)
                    // Serialisiere den Spieler in ein String
                    playerStr = NetworkClient.Serialize(player);

                    if (hostStr.Equals(playerStr)) // Wenn Host, dann auf Index 0 setzen
                        participants.Insert(0, player);
                    else // Wenn kein Host, einfach hinzufügen
                        participants.Add(player);
                }

                // Lokaler Spieler
                playerStr = NetworkClient.Serialize(LocalPlayer);
                if (hostStr.Equals(playerStr)) // Wenn Host, dann auf Index 0 setzen
                    participants.Insert(0, LocalPlayer);
                else 
                    participants.Add(LocalPlayer); // Wenn kein Host, einfach hinzufügen
            }
            Program.LoadGame(CurrentLobby.MapName, new NetworkHandler(
                DiscoveryClient.UdpClient, participants, LocalPlayer
            ) { Lobby = CurrentLobby });
        }

        private void UpdateInfo()
        { // Befehl zum aktualisieren der Lobbyinformationen
            if (DiscoveryClient == null || !DiscoveryClient.LoggedIn) return;

            CurrentLobby = null;
            foreach(NetworkLobby lobby in DiscoveryClient.Lobbies)
            {
                if (lobby.PlayerExists(LocalPlayer))
                    CurrentLobby = lobby;
            }

            DiscoveryStatus = null;
            LobbyListScreen.UpdateLobbies(DiscoveryClient.Lobbies);
            LobbyScreen.Lobby = CurrentLobby;

            // Wenn in keiner lobby, zeige die lobby liste
            // andernfalls, zeige die lobby informationen
            ForegroundScreen = CurrentLobby == null ? 
                (GuiComponent)LobbyListScreen : LobbyScreen;
        }
        #endregion

        #region Implementation region
        public override void Update()
        {
            backgroundGame.Update();
            ForegroundScreen.Update();
        }

        public override void Render(Graphics g)
        {
            // Spielszene rendern und dimmen
            backgroundGame.Render(g);
            g.FillRectangle(BACKGROUND_COLOR, 
                0, 0, StaticEngine.RenderWidth, StaticEngine.RenderHeight);
            ForegroundScreen.Render(g);

            if (DiscoveryStatus != null)
            { // Zeichne den Status zum Vermittlungsserver
                Size textSize = TextRenderer.MeasureText(DiscoveryStatus, StandartText1Font);
                g.DrawString(DiscoveryStatus, StandartText1Font, Brushes.Red, 0, StaticEngine.RenderHeight - textSize.Height);
            }
        }

        public override void KeyUp(KeyEventArgs e) => ForegroundScreen.KeyUp(e);
        public override void KeyDown(KeyEventArgs e) => ForegroundScreen.KeyDown(e);

        public override void MouseUp(MouseEventArgs e) => ForegroundScreen.MouseUp(e);
        public override void MouseDown(MouseEventArgs e) => ForegroundScreen.MouseDown(e);
        public override void MouseMove(MouseEventArgs e) => ForegroundScreen.MouseMove(e);
        public override void MouseWheel(MouseEventArgs e) => ForegroundScreen.MouseWheel(e);
        #endregion

        internal static ButtonContainer CreateButton(string text, Rectangle bounds)
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
