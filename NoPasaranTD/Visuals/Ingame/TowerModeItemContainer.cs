using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    public class TowerModeItemContainer : ItemContainer<TowerTargetMode>
    {
        public override TowerTargetMode DataContext { get; set; }
        public override Point Position { get => Bounds.Location; set => Bounds = new Rectangle(value, ItemSize); }
        public override Size ItemSize { get => Bounds.Size; set => Bounds = new Rectangle(Position, value); }

        public override void TranslateTransform(int offX, int offY)
            => Position += new Size(offX, offY);

        private SolidBrush normalBackground = new SolidBrush(Color.FromArgb(159, 161, 166));
        private SolidBrush selectedBackground = new SolidBrush(Color.FromArgb(99, 124, 186));
        private SolidBrush hoverBackground = new SolidBrush(Color.FromArgb(145, 155, 179));
        private Font textFont = GuiComponent.StandartText1Font;
        private int borderWidth = 2;

        private RectangleF InnerBounds => new RectangleF(Bounds.X + borderWidth, Bounds.Y + borderWidth, Bounds.Width - borderWidth * 2, Bounds.Height - borderWidth * 2);

        public override void Render(Graphics g)
        {
            if (IsSelected)
                g.FillRectangle(selectedBackground, Bounds);

            if (IsMouseOver)
                g.FillRectangle(hoverBackground, InnerBounds);
            else
                g.FillRectangle(normalBackground, InnerBounds);

            Size priceSize = TextRenderer.MeasureText(Enum.GetName(typeof(TowerTargetMode),DataContext), textFont);
            g.DrawString(Enum.GetName(typeof(TowerTargetMode), DataContext), textFont, Brushes.Black, Bounds.X + Bounds.Width / 2 - priceSize.Width / 2, Bounds.Y + Bounds.Height - priceSize.Height - 5);

        }
    }
}
