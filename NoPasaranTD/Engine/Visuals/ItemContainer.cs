using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Engine.Visuals
{
    public abstract class ItemContainer<T>
    {
        public abstract T DataContext { get; set; }
        public abstract Point Position { get; set; }
        public abstract Size ItemSize { get; set; }
        public Graphics Graphics { get; set; }
        public abstract void Draw(Size size, Point point);
        public abstract void Draw();
    }

    public class TowerItemContainer : ItemContainer<Tower>
    {
        private Rectangle background = new Rectangle();            
        public Brush BackgroundBrush { get; set; } = Brushes.Red;        
        public override Tower DataContext { get; set; }
        public override Point Position { get { return new Point(background.X, background.Y); } set { background.X = value.X; background.Y = value.Y; } }
        public override Size ItemSize { get { return new Size(background.Width, background.Height); } set { background.Width = value.Width; background.Height = value.Height; } }

        public override void Draw(Size size, Point point)
        {
            ItemSize = size;
            Position = point;
            DrawItem();
        }

        public override void Draw()
        {
            DrawItem();
        }

        void DrawItem()
        {
            Graphics.FillRectangle(BackgroundBrush, background);
        }
    }
}
