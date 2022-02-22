using NoPasaranTD.Data;
using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Engine.Visuals
{
    public class UILayout
    {


        public ListContainer<Tower, TowerItemContainer> TowerBuildMenu { get; set; } = new ListContainer<Tower, TowerItemContainer>()
        {
            Margin = 10,
            ItemSize = new System.Drawing.Size(100, 130),
            Position = new System.Drawing.Point(20, Engine.RenderHeight - 150),
            ContainerSize = new System.Drawing.Size(Engine.RenderWidth - 40, 130),
            BackgroundColor = Brushes.SlateGray,
            Items = new NotifyCollection<Tower>()
            {
                new TowerTest(),
                new TowerTest(),
                new TowerTest(),
                new TowerTest(),
                new TowerTest(),
            },            
        };

        private DragDropService placingTowerDragDrop = new DragDropService();
        private Rectangle towerDragDropVisual = new Rectangle(Engine.RenderWidth / 2, Engine.RenderHeight/2,0,0);

        public UILayout()
        {
            TowerBuildMenu.DefineItems();
            TowerBuildMenu.SelectionChanged += TowerBuildMenu_SelectionChanged;
            placingTowerDragDrop.DragDropFinish += PlacingTowerDragDrop_DragDropFinish;

            Engine.OnRender += Render;
        }

        private void PlacingTowerDragDrop_DragDropFinish(DragDropArgs args)
        {
            // TODO: Tower Plazieren
        }

        private void TowerBuildMenu_SelectionChanged()
        {
            placingTowerDragDrop.Start(new Rectangle(Engine.RenderWidth / 2, Engine.RenderHeight -50, 50, 50));
        }

        private void Render(Graphics g)
        {
            if (placingTowerDragDrop.IsMoving)
                g.FillRectangle(Brushes.Red, placingTowerDragDrop.MovedObject);
            
        }
    }
}
