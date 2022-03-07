using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class GuiLobby : GuiComponent
    {

        private static readonly SolidBrush BACKGROUND_COLOR = new SolidBrush(Color.FromArgb(75, Color.Black));

        private ButtonContainer btnCreateLobby { get; }
        private ButtonContainer btnJoinLobby { get; }

        private Game backgroundGame;
        public GuiLobby()
        {
            // Lade Spielszene
            Map map = MapData.GetMapByFile("test2"); map.Initialize();
            backgroundGame = new Game(map);
            {
                // UILayout unsichtbar und inaktiv schalten
                backgroundGame.UILayout.Active = false;
                backgroundGame.UILayout.Visible = false;

                // Spawne Türme in Spielszene
                Size towerSize = StaticInfo.GetTowerSize(typeof(TowerCanon));
                backgroundGame.AddTower(new TowerCanon() { Hitbox = new Rectangle(new Point(325, 200), towerSize) });
                backgroundGame.AddTower(new TowerCanon() { Hitbox = new Rectangle(new Point(215, 250), towerSize) });
                backgroundGame.AddTower(new TowerCanon() { Hitbox = new Rectangle(new Point(30, 45), towerSize) });
                backgroundGame.AddTower(new TowerCanon() { Hitbox = new Rectangle(new Point(435, 65), towerSize) });
            }

            btnCreateLobby = CreateButton("Create Lobby", new Rectangle(25, StaticEngine.RenderHeight - 95, 125, 30));
            btnJoinLobby = CreateButton("Join Lobby", new Rectangle(25, StaticEngine.RenderHeight - 55, 125, 30));
        }

        public override void Update() => backgroundGame.Update();

        public override void Render(Graphics g)
        {
            // Spielszene rendern und dimmen
            backgroundGame.Render(g);
            g.FillRectangle(BACKGROUND_COLOR, 
                0, 0, StaticEngine.RenderWidth, StaticEngine.RenderHeight);

            btnCreateLobby.Render(g);
            btnJoinLobby.Render(g);
        }

        public override void KeyUp(KeyEventArgs e) => backgroundGame.KeyUp(e);
        public override void KeyDown(KeyEventArgs e) => backgroundGame.KeyDown(e);

        public override void MouseUp(MouseEventArgs e) => backgroundGame.MouseUp(e);
        public override void MouseDown(MouseEventArgs e)
        {
            backgroundGame.MouseDown(e);
            btnCreateLobby.MouseDown(e);
            btnJoinLobby.MouseDown(e);
        }

        public override void MouseMove(MouseEventArgs e) => backgroundGame.MouseMove(e);
        public override void MouseWheel(MouseEventArgs e) => backgroundGame.MouseWheel(e);

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
