using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    /// <summary>
    /// Das UI Layout innerhalb eines Gameplays </br>
    /// Enthält: Tower Baumenü, Tower Platzierung 
    /// </summary>
    public class UILayout : GuiComponent
    {
        // Game instanz in dem das UI Layout zu finden ist
        private Game game;

        /// <summary>
        /// Das Baumenü 
        /// </summary>
        public ListContainer<Tower, TowerItemContainer> TowerBuildMenu { get; set; } = new ListContainer<Tower, TowerItemContainer>()
        {
            Margin = 10,
            Orientation = Orientation.Horizontal,
            ItemSize = new System.Drawing.Size(100, 110),
            Position = new System.Drawing.Point(120, StaticEngine.RenderHeight - 150),
            ContainerSize = new System.Drawing.Size(StaticEngine.RenderWidth - 150, 130),
            BackgroundColor = new SolidBrush(Color.FromArgb(240, 132, 140, 156)),
            // Spezifizierung der Verschiedenen Towers
            Items = new NotifyCollection<Tower>()
            {
                new TowerCanon(),
                new TowerArtillery(),
            },            
        };

        /// <summary>
        /// Die Übersicht der Daten des ausgewählten Towers
        /// </summary>
        public TowerDetailsContainer TowerDetailsContainer { get; set; } = new TowerDetailsContainer()
        {
            Bounds = new System.Drawing.Rectangle(StaticEngine.RenderWidth-250,5,240,450),        
            Background = new SolidBrush(Color.FromArgb(240, 132, 140, 156)),
            ButtonFont = GuiComponent.StandartText1Font,
            Visible = false,
            Foreground = Brushes.Black,
            TextFont = GuiComponent.StandartText1Font,
        };

        /// <summary>
        /// Expandiert oder Minimiert das Kaufmenü
        /// </summary>
        public ButtonContainer HideBuildMenuContainer { get; set; } = new ButtonContainer()
        {
            Bounds = new Rectangle(20, StaticEngine.RenderHeight - 150, 80, 130),
            StringFont = GuiComponent.StandartIconFont,
            Foreground = Brushes.Black,
            Content = "←",
            BorderBrush = new SolidBrush(Color.FromArgb(230, 128, 138, 189)),
            Background = new SolidBrush(Color.FromArgb(240, 132, 140, 156)),
            Margin = 1         
        };

        /// <summary>
        /// Zeigt alle derzeitigen Spieler in der Session an (Anzeige ist raus!)
        /// </summary>
        public PlayerListContainer PlayerListContainer { get; set; } = new PlayerListContainer()
        {
            Bounds = new Rectangle(StaticEngine.RenderWidth/2 - 100, StaticEngine.RenderHeight/2 - 125, 200, 250),
            Background = new SolidBrush(Color.FromArgb(230, 132, 140, 156)),
            BorderBrush = new SolidBrush(Color.FromArgb(150, 132, 140, 156)),
            Visible = false,
        };

        public OptionsContainer OptionsContainer { get; set; } = new OptionsContainer()
        {
            Bounds = new Rectangle(StaticEngine.RenderWidth/2 - 150,5,290,50),
            Background = new SolidBrush(Color.FromArgb(150, 132, 140, 156)),
            Foreground = Brushes.Black,
        };

        public ChatContainer ChatContainer { get; set; } = new ChatContainer()
        {           
            Visible = false,
            Bounds = new Rectangle(StaticEngine.RenderWidth/2 - 150, StaticEngine.RenderHeight/2 - 200, 300, 400),
            Background = new SolidBrush(Color.FromArgb(230, 132, 140, 156)),
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
            // Initialisiert alle UI Komponenten
            TowerBuildMenu.DefineItems();
            TowerDetailsContainer.Init(gameObj);
            PlayerListContainer.Init(gameObj);
            ChatContainer.Init(gameObj);
            OptionsContainer.Init(gameObj);

            // Initialisiert alle Events
            TowerBuildMenu.SelectionChanged += TowerBuildMenu_SelectionChanged;
            placingTowerDragDrop.DragDropFinish += PlacingTowerDragDrop_DragDropFinish;
            HideBuildMenuContainer.ButtonClicked += HideBuildMenüButton_ButtonClicked;

            // Verweist alle GUI Components
            GetGUIComponents(this,typeof(UILayout));

            game = gameObj;
        }

        private async void HideBuildMenüButton_ButtonClicked()
        {
            // Animation zum einklappen des Buildmenüs
            if (TowerBuildMenu.Visible)
            {
                HideBuildMenuContainer.Content = "→";
                while (TowerBuildMenu.Bounds.Width > 0)
                {
                    TowerBuildMenu.Bounds = new Rectangle(TowerBuildMenu.Bounds.X, TowerBuildMenu.Bounds.Y, TowerBuildMenu.Bounds.Width - 130, TowerBuildMenu.Bounds.Height);
                    await Task.Delay(1);
                }
                TowerBuildMenu.Bounds = new Rectangle(TowerBuildMenu.Bounds.X, TowerBuildMenu.Bounds.Y, 0, TowerBuildMenu.Bounds.Height);
                TowerBuildMenu.Visible = false;               
            }
            else // Animation zum ausklappen des Buildmenüs
            {
                HideBuildMenuContainer.Content = "←";
                TowerBuildMenu.Visible = true;
                while (TowerBuildMenu.Bounds.Width < StaticEngine.RenderWidth - 40)
                {
                    TowerBuildMenu.Bounds = new Rectangle(TowerBuildMenu.Bounds.X, TowerBuildMenu.Bounds.Y, TowerBuildMenu.Bounds.Width + 130, TowerBuildMenu.Bounds.Height);
                    await Task.Delay(1);
                }
                
                TowerBuildMenu.Bounds = new Rectangle(TowerBuildMenu.Bounds.X, TowerBuildMenu.Bounds.Y, StaticEngine.RenderWidth - 140, TowerBuildMenu.Bounds.Height);
                
            }
        }

        // Wird beim abschließen des DragDrop Vorganges ausgelöst
        private void PlacingTowerDragDrop_DragDropFinish(DragDropArgs args)
        {
            if (TowerBuildMenu.Bounds.IntersectsWith(args.MovedObject)) return;
            if (!game.IsTowerValidPosition(args.MovedObject)) return;

            Tower tower = null;
            if (args.Context is TowerCanon) tower = new TowerCanon();
            if (args.Context is TowerArtillery) tower = new TowerArtillery();
            // TODO: Towers Spezifizeiren
            if (tower != null && (StaticInfo.GetTowerPrice(tower.GetType()) <= game.Money || game.GodMode))
            {
                tower.Hitbox = args.MovedObject;
                game.NetworkHandler.InvokeEvent("AddTower", tower);
            }
        }

        private void TowerBuildMenu_SelectionChanged()
        {
            Tower tower = null;

            if (TowerBuildMenu.SelectedItem is TowerCanon)
                tower = new TowerCanon();
            else if (TowerBuildMenu.SelectedItem is TowerArtillery)
                tower = new TowerArtillery();
           
            if (tower != null)
            {
                tower.Hitbox = new Rectangle(new Point(StaticEngine.MouseX, StaticEngine.MouseY), StaticInfo.GetTowerSize(tower.GetType()));
                placingTowerDragDrop.Context = tower;
                placingTowerDragDrop.Start(tower.Hitbox);
            }
            // TODO: Größe des Rechteckes auf TowerType spezifieren           
        }

        public override void Update()
        {
            if (!Visible) return;
            placingTowerDragDrop.Update();
            TowerBuildMenu.Update();
            PlayerListContainer.Update();
            OptionsContainer.Update();
            ChatContainer.Update();
        }

        public override void Render(Graphics g)
        {
            if (!Visible) return;
            TowerDetailsContainer.Render(g);   
            TowerBuildMenu.Render(g);
            HideBuildMenuContainer.Render(g);
            PlayerListContainer.Render(g);
            OptionsContainer.Render(g);
            ChatContainer.Render(g);

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

                    // Überprüft ob die derzeitige Position valide für eine Platzierung wäre
                    if (!game.IsTowerValidPosition(placingTowerDragDrop.MovedObject))
                        ((Tower)placingTowerDragDrop.Context).IsPositionValid = false;
                    else
                        ((Tower)placingTowerDragDrop.Context).IsPositionValid = true;

                    ((Tower)placingTowerDragDrop.Context).Render(g);
                }
            }

        }

        public override void KeyUp(KeyEventArgs e)
        {
            if (!Visible) return;
            TowerBuildMenu.KeyUp(e);
        }

        public override void KeyPress(KeyPressEventArgs e)
        {
            if (!Visible) return;
            TowerBuildMenu.KeyPress(e);
            HideBuildMenuContainer.KeyPress(e);
            ChatContainer.KeyPress(e);
        }

        public override void KeyDown(KeyEventArgs args)
        {
            if (!Visible) return;
            TowerBuildMenu.KeyDown(args);
            HideBuildMenuContainer.KeyDown(args);
            ChatContainer.KeyDown(args);
        }

        public override void MouseUp(MouseEventArgs e)
        {
            if (!Visible) return;
            TowerBuildMenu.MouseUp(e);
            placingTowerDragDrop.MouseUp(e);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            if (!Visible) return;
            TowerBuildMenu.MouseDown(e);
            TowerDetailsContainer.MouseDown(e);
            placingTowerDragDrop.MouseDown(e);
            OptionsContainer.MouseDown(e);
            ChatContainer.MouseDown(e);

            foreach (var item in game.Towers)
            {
                if (!IsMouseOnUI() && item.Hitbox.Contains(e.Location))
                {
                    if (SelectedTower != null)
                        SelectedTower.IsSelected = false;

                    SelectedTower = item;
                    TowerDetailsContainer.Visible = true;
                    TowerDetailsContainer.Context = item;
                    SelectedTower.IsSelected = true;
                }
            }
            HideBuildMenuContainer.MouseDown(e);
        }

        public override void MouseMove(MouseEventArgs e)
        {
            if (!Visible) return;
            TowerBuildMenu.MouseMove(e);
            ChatContainer.MouseMove(e);
        }

        public override void MouseWheel(MouseEventArgs e)
        {
            if (!Visible) return;
            TowerBuildMenu.MouseWheel(e);
            ChatContainer.MouseWheel(e);
        }

        void DrawGameStats(Graphics g)
        {
            // DAS BLEIBT ALLES SO WIE ES HIER IST!!!

            // Die Kontostandanzeige des derzeitigen Spieles
            g.DrawString(game.GodMode ? "∞₿" : game.Money + "₿",GuiComponent.StandartHeader1Font, new SolidBrush(Color.FromArgb(200, 24, 24, 24)), 0,0);         
            // Die Lebensanzeige des derzeitigen Spieles
            g.DrawString(game.GodMode ? "∞♥" : game.HealthPoints + "♥", GuiComponent.StandartHeader1Font, new SolidBrush(Color.FromArgb(200, 24, 24, 24)), 150, 0);
            // Die Zahl der derzeitigen Runde
            g.DrawString(game.Round + ". Round", GuiComponent.StandartHeader1Font, new SolidBrush(Color.FromArgb(200, 24, 24, 24)), 300, 0);
        }        
    }
}
