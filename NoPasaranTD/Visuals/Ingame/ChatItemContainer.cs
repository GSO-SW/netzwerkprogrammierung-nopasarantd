using System.Drawing;
using System.Windows.Forms;

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
            get => new Point(Bounds.X, Bounds.Y);
            set { Bounds = new Rectangle(value, Bounds.Size); PositionChanged(); }
        }

        /// <summary>
        /// Größe des Items
        /// </summary>
        public override Size ItemSize
        {
            get => new Size(Bounds.Width, Bounds.Height);
            set
            {
                // Passt die Messagebox Höhe anhand der Textgröße an
                Size textSize = TextRenderer.MeasureText(DataContext, StandartText3Font, new Size(value.Width, int.MaxValue), TextFormatFlags.WordBreak);
                Bounds = new Rectangle(Position, new Size(value.Width, (textSize.Height + (textSize.Width / value.Width) * textSize.Height)));
            }
        }

        // Prüft ob sich der Container im Sichtbaren Bereich befindet. Wenn ja dann darf dieser gezeichnet werden, wenn nicht dann nicht.
        private void PositionChanged()
        {
            Visible = Position.X + ItemSize.Width >= ParentBounds.X && Position.X <= ParentBounds.X + ParentBounds.Width;
        }

        public override void TranslateTransform(int offX, int offY)
        {
            Position = new Point(offX + Position.X, offY + Position.Y);
        }

        public override void Render(Graphics g)
        {
            g.FillRectangle(Brushes.Beige, Bounds);
            g.DrawString(DataContext, StandartText3Font, Brushes.Black, Bounds);
        }
    }
}
