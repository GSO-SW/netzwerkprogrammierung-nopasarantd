using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Visuals.Ingame
{
    public class ChatItemContainer : ItemContainer<string>
    {
        public override string DataContext { get; set; }

        /// <summary>
        /// Position des Item-Containers auf dem Screen
        /// </summary>
        public override Point Position
        {
            get { return new Point(Bounds.X, Bounds.Y); }
            set { Bounds = new Rectangle(value, ItemSize); PositionChanged(); }
        }

        /// <summary>
        /// Größe des Items
        /// </summary>
        public override Size ItemSize
        {
            get { return new Size(Bounds.Width, Bounds.Height); }
            set { Bounds = new Rectangle(Position, value); }
        }

        // Prüft ob sich der Container im Sichtbaren Bereich befindet. Wenn ja dann darf dieser gezeichnet werden, wenn nicht dann nicht.
        private void PositionChanged()
            => Visible = Position.X + ItemSize.Width >= ParentBounds.X && Position.X <= ParentBounds.X + ParentBounds.Width;


        public override void TranslateTransform(int offX, int offY) =>
            Position = new Point(offX + Position.X, offY + Position.Y);

        public override void Render(Graphics g)
        {
            g.FillRectangle(Brushes.Beige, Bounds);
            g.DrawString(DataContext, StandartText1Font, Brushes.Black, Bounds);
        }
    }
}
