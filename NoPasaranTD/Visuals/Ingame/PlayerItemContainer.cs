using NoPasaranTD.Networking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    public class PlayerItemContainer : ItemContainer<Networking.NetworkClient>
    {
        public override NetworkClient DataContext { get; set; }
        public override Point Position { get; set; }
        public override Size ItemSize { get; set; }
        public SolidBrush Background { get; set; } = new SolidBrush(Color.FromArgb(175, 183, 219));
        public SolidBrush Foreground { get; set; } = new SolidBrush(Color.Black);
        public SolidBrush HighlightBackground { get; set; } = new SolidBrush(Color.FromArgb(133, 149, 222));
        public Font ForegroundFont { get; set; } = StandartText2Font;

        public override void TranslateTransform(int offX, int offY)
        {
            
        }

        public override void Update()
        {
            Bounds = new Rectangle(Position, ItemSize);
        }

        public override void Render(Graphics g)
        {
            if (IsMouseOver)
                g.FillRectangle(HighlightBackground, Bounds);
            else
                g.FillRectangle(Background, Bounds);

            
            Size nameSize = TextRenderer.MeasureText(DataContext.Name,ForegroundFont);
            g.DrawString(DataContext.Name, ForegroundFont, Foreground, new Point(Bounds.X + 5, Bounds.Y + (Bounds.Height / 2) - nameSize.Height / 2));
        }
    }
}
