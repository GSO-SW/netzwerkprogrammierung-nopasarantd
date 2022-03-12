using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using NoPasaranTD.Networking;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class GuiLobby : GuiComponent
    {

        private static readonly SolidBrush BACKGROUND_COLOR = new SolidBrush(Color.FromArgb(75, Color.Black));

        private ListContainer<object, LobbyItemContainer> lobbyList { get; }
        private ButtonContainer btnCreateLobby { get; }

        private Game backgroundGame;
        public GuiLobby()
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

            lobbyList = new ListContainer<object, LobbyItemContainer>()
            {
                Margin = 10,
                ItemSize = new Size((int)(StaticEngine.RenderWidth / 1.5f) - 20, 100),

                Position = new Point(StaticEngine.RenderWidth / 6, StaticEngine.RenderHeight / 6 - 20),
                ContainerSize = new Size((int)(StaticEngine.RenderWidth / 1.5f), (int)(StaticEngine.RenderHeight / 1.5f)),
                BackgroundColor = new SolidBrush(Color.FromArgb(225, Color.Gray)),

                Items = new NotifyCollection<object>()
            };

            for(int i = 0; i < 5; i++)
                lobbyList.Items.Add(i);
            lobbyList.DefineItems();

            btnCreateLobby = CreateButton("Create Lobby", new Rectangle(
                lobbyList.Position.X + lobbyList.ContainerSize.Width - 150, 
                lobbyList.Position.Y + lobbyList.ContainerSize.Height + 5, 
                150, 30
            ));

            btnCreateLobby.ButtonClicked += () => Program.LoadGame("spentagon");
        }

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

            btnCreateLobby.Render(g);
            lobbyList.Render(g);
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
