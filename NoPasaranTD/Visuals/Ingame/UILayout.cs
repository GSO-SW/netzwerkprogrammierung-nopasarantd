using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    /// <summary>
    /// Das UI Layout innerhalb eines Gameplays </br>
    /// Enthält: Tower Baumenü, Tower Platzierung 
    /// </summary>
    public class UILayout
    {
        // Game instanz in dem das UI Layout zu finden ist
        private Game game;

        /// <summary>
        /// Das Baumenü 
        /// </summary>
        public ListContainer<Tower, TowerItemContainer> TowerBuildMenu { get; set; } = new ListContainer<Tower, TowerItemContainer>()
        {
            Margin = 10,
            ItemSize = new System.Drawing.Size(100, 130),
            Position = new System.Drawing.Point(20, StaticEngine.RenderHeight - 150),
            ContainerSize = new System.Drawing.Size(StaticEngine.RenderWidth - 40, 130),
            BackgroundColor = new SolidBrush(Color.FromArgb(250, 143, 167, 186)),
            // Spezifizierung der Verschiedenen Towers
            Items = new NotifyCollection<Tower>()
            {
                new TowerCanon(),
            },            
        };

        /// <summary>
        /// Die Übersicht der Daten des ausgewählten Towers
        /// </summary>
        public TowerDetailsContainer TowerDetailsContainer { get; set; } = new TowerDetailsContainer()
        {
            Bounds = new System.Drawing.Rectangle(StaticEngine.RenderWidth-250,5,240,400),        
            Background = new SolidBrush(Color.FromArgb(250,143, 167, 186)),
            ButtonFont = GuiComponent.StandartText1Font,
            Visible = false,
            Foreground = Brushes.Black,
            TextFont = GuiComponent.StandartText1Font,
        };

        // Drag Drop Service für das platzieren eines neuen Towers auf dem Bildschirm
        private DragDropService placingTowerDragDrop = new DragDropService();
        
        private Tower selectedTower = null;
        /// <summary>
        /// Der Ausgweählte Tower. Wird beim draufklicken zugewiesen
        /// </summary>
        public Tower SelectedTower { get { return selectedTower; } set { selectedTower = value; } }

        public UILayout(Game gameObj)
        {
            TowerBuildMenu.DefineItems();
            TowerDetailsContainer.Init(gameObj);

            TowerBuildMenu.SelectionChanged += TowerBuildMenu_SelectionChanged;
            placingTowerDragDrop.DragDropFinish += PlacingTowerDragDrop_DragDropFinish;

            game = gameObj;
        }

        // Wird beim abschließen des DragDrop Vorganges ausgelöst
        private void PlacingTowerDragDrop_DragDropFinish(DragDropArgs args)
        {
            if (TowerBuildMenu.Bounds.IntersectsWith(args.MovedObject))
                return;

            if (!game.IsTowerValidPosition(args.MovedObject))
                return;

            if (args.Context is TowerCanon && StaticInfo.GetTowerPrice(typeof(TowerCanon)) <= game.Money)
            {
                game.AddTower(new TowerCanon() { Hitbox = args.MovedObject });
                game.Money -= (int)StaticInfo.GetTowerPrice(typeof(TowerCanon));
            }
            // TODO: Towers Spezifizeiren
        }

        private void TowerBuildMenu_SelectionChanged()
        {
            Tower tower = null;

            if (TowerBuildMenu.SelectedItem is TowerCanon)
                tower = new TowerCanon();
           
            if (tower != null)
            {
                tower.Hitbox = new Rectangle(new Point(StaticEngine.MouseX, StaticEngine.MouseY), StaticInfo.GetTowerSize(tower.GetType()));
                placingTowerDragDrop.Context = tower;
                placingTowerDragDrop.Start(tower.Hitbox);
            }
            // TODO: Größe des Rechteckes auf TowerType spezifieren           
        }

        public void Update()
        {
            placingTowerDragDrop.Update();
            TowerBuildMenu.Update();
        }

        public void Render(Graphics g)
        {
            TowerDetailsContainer.Render(g);   
            TowerBuildMenu.Render(g);

            DrawGameStats(g);

            // TODO: Testcode, ausgewählter Tower soll gerendert werden
            // unabhängig davon ob er bewegt wird oder nicht!
            // Bei bewegen ins Spielfeld, nur die Alpha etwas runterdrehen.
            // Bei platzieren das Alpha wieder auf normal setzen und den Tower auf diese Position zeichnen
            if (placingTowerDragDrop.Context != null)
            {
                if (placingTowerDragDrop.IsMoving)
                {
                    ((Tower)placingTowerDragDrop.Context).Hitbox = placingTowerDragDrop.MovedObject;
                    ((Tower)placingTowerDragDrop.Context).Render(g);
                }
            }
        }

        public void KeyUp(KeyEventArgs e) => TowerBuildMenu.KeyUp(e);
        public void KeyDown(KeyEventArgs args) => TowerBuildMenu.KeyDown(args);

        public void MouseUp(MouseEventArgs e)
        {
            TowerBuildMenu.MouseUp(e);
            placingTowerDragDrop.MouseUp(e);
        }

        public void MouseDown(MouseEventArgs e)
        {
            TowerBuildMenu.MouseDown(e);
            TowerDetailsContainer.MouseDown(e);
            placingTowerDragDrop.MouseDown(e);

            foreach (var item in game.Towers)
            {
                if (item.Hitbox.Contains(e.Location))
                {
                    SelectedTower = item;
                    TowerDetailsContainer.Visible = true;
                    TowerDetailsContainer.Context = item;
                }
            }
        }

        public void MouseMove(MouseEventArgs e) => TowerBuildMenu.MouseMove(e);

        void DrawGameStats(Graphics g)
        {
            // Die Kontostandanzeige des derzeitigen Spieles
            g.DrawString(game.Money + "₿",GuiComponent.StandartHeader1Font, new SolidBrush(Color.FromArgb(200, 24, 24, 24)), 0,0);         
            // Die Lebensanzeige des derzeitigen Spieles
            g.DrawString("x" + "♥", GuiComponent.StandartHeader1Font, new SolidBrush(Color.FromArgb(200, 24, 24, 24)), 150, 0);
            // Die Zahl der derzeitigen Runde
            g.DrawString("x" + ". Round", GuiComponent.StandartHeader1Font, new SolidBrush(Color.FromArgb(200, 24, 24, 24)), 300, 0);
        }        
    }
}
