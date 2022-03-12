﻿using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using NoPasaranTD.Networking;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class GuiMainMenu : GuiComponent
    {

        private static readonly SolidBrush BACKGROUND_COLOR = new SolidBrush(Color.FromArgb(75, Color.Black));

        public string DiscoveryStatus { get; private set; }
        public DiscoveryClient DiscoveryClient { get; private set; }
        public NetworkLobby CurrentLobby { get; private set; }

        public NetworkClient LocalPlayer { get; set; }
        public GuiComponent ForegroundScreen { get; set; }
        public LobbyListScreen LobbyListScreen { get; }
        public LobbyScreen LobbyScreen { get; }

        private readonly Game backgroundGame;
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

            LobbyScreen = new LobbyScreen(this);
            LobbyListScreen = new LobbyListScreen(this);
            ForegroundScreen = LobbyListScreen;

            // Initialisiere Verbindung mit Vermittlungsserver
            Task.Run(OpenDiscovery);
        }

        private void OpenDiscovery()
        {
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

        #region Discovery event region
        private void StartGame()
        {
            if (DiscoveryClient == null || !DiscoveryClient.LoggedIn) return;
            Program.LoadGame("spentagon", new NetworkHandler(
                DiscoveryClient.UdpClient, DiscoveryClient.Clients
            ));
        }

        private void UpdateInfo()
        {
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
            {
                Size textSize = TextRenderer.MeasureText(DiscoveryStatus, StandartText1Font);
                g.DrawString(DiscoveryStatus, StandartText1Font, Brushes.Red, 0, StaticEngine.RenderHeight - textSize.Height);
            }
        }

        public override void KeyUp(KeyEventArgs e)
        {
            backgroundGame.KeyUp(e);
            ForegroundScreen.KeyUp(e);
        }

        public override void KeyDown(KeyEventArgs e)
        {
            backgroundGame.KeyDown(e);
            ForegroundScreen.KeyDown(e);
        }

        public override void MouseUp(MouseEventArgs e)
        {
            backgroundGame.MouseUp(e);
            ForegroundScreen.MouseUp(e);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            backgroundGame.MouseDown(e);
            ForegroundScreen.MouseDown(e);
        }

        public override void MouseMove(MouseEventArgs e)
        {
            backgroundGame.MouseMove(e);
            ForegroundScreen.MouseMove(e);
        }

        public override void MouseWheel(MouseEventArgs e)
        {
            backgroundGame.MouseWheel(e);
            ForegroundScreen.MouseWheel(e);
        }
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
