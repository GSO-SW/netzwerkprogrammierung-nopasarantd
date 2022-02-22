using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Engine.Visuals
{
    public abstract class ItemContainer<T>
    {
        public abstract T DataContext { get; set; }
        public abstract Point Position { get; set; }
        public abstract Size ItemSize { get; set; }
        public Graphics Graphics { get; set; }
        public bool IsMouseOver { get; set; } = false;
    }

    public class TowerItemContainer : ItemContainer<Tower>
    {
        public TowerItemContainer()
        {
            Engine.OnRender += DrawItem;
            Engine.OnMouseMove += MouseMove; 
        }

        private Rectangle background = new Rectangle();            
        public Brush BackgroundBrush { get; set; } = Brushes.Red;        
        public override Tower DataContext { get; set; }
        public override Point Position { get { return new Point(background.X, background.Y); } set { background.X = value.X; background.Y = value.Y; } }
        public override Size ItemSize { get { return new Size(background.Width, background.Height); } set { background.Width = value.Width; background.Height = value.Height; } }

        private void DrawItem(Graphics g)
        {
            g.FillRectangle(BackgroundBrush, background);
        }

        private void MouseMove(MouseEventArgs args)
        {
            if (background.Contains(new Point(Engine.MouseX, Engine.MouseY)))
                IsMouseOver = true;
            else
                IsMouseOver = false;
        }
    }
}
