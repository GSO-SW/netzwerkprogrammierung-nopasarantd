using NoPasaranTD.Data;
using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Engine.Visuals
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
            Position = new System.Drawing.Point(20, Engine.RenderHeight - 150),
            ContainerSize = new System.Drawing.Size(Engine.RenderWidth - 40, 130),
            BackgroundColor = Brushes.SlateGray,
            // Spezifizierung der Verschiedenen Towers
            Items = new NotifyCollection<Tower>()
            {
                new TowerCanon(),
            },            
        };

        public TowerDetailsContainer TowerDetailsContainer { get; set; } = new TowerDetailsContainer()
        {

        };
       
        // Drag Drop Service für das platzieren eines neuen Towers auf dem Bildschirm
        private DragDropService placingTowerDragDrop = new DragDropService();
        
        private Tower selectedTower = null;
        public Tower SelectedTower { get { return selectedTower; } set { selectedTower = value; SelectedTowerChanged(); } }

        public UILayout(Game gameObj)
        {
            TowerBuildMenu.DefineItems();

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

            if (args.Context != null)
                game.AddTower((Tower)args.Context);
             
            // TODO: Towers Spezifizeiren           
        }

        private void TowerBuildMenu_SelectionChanged()
        {
            Tower tower = null;

            if (TowerBuildMenu.SelectedItem is TowerCanon)
                tower = new TowerCanon();
           
            if (tower != null)
            {
                tower.Hitbox = new Rectangle(new Point(Engine.MouseX, Engine.MouseY), StaticInfo.GetTowerSize(tower.GetType()));
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
            TowerBuildMenu.Render(g);
            TestButton.Render(g);
            DrawGameStats(g);

            // TODO: Testcode, ausgewählter Tower soll gerendert werden
            // unabhängig davon ob er bewegt wird oder nicht!
            // Bei bewegen ins Spielfeld, nur die Alpha etwas runterdrehen.
            // Bei platzieren das Alpha wieder auf normal setzen und den Tower auf diese Position zeichnen
            // TODO ÜBERARBEITET: Alpha in der Tower Render Methode nach validierung einstellen
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
            placingTowerDragDrop.MouseDown(e);
        }

        public void MouseMove(MouseEventArgs e) => TowerBuildMenu.MouseMove(e);

        private void SelectedTowerChanged()
        {

        }

        void DrawGameStats(Graphics g)
        {
            g.DrawString(game.Money + "₿",GuiComponent.StandartHeader1Font, new SolidBrush(Color.FromArgb(200, 24, 24, 24)), 0,0);           
            g.DrawString(game.Health + "♥", GuiComponent.StandartHeader1Font, new SolidBrush(Color.FromArgb(200, 24, 24, 24)), 150, 0);
            g.DrawString(game.CurrentRound + ". Round", GuiComponent.StandartHeader1Font, new SolidBrush(Color.FromArgb(200, 24, 24, 24)), 300, 0);
        }

        
    }
}
