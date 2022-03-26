using NoPasaranTD.Networking;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    /// <summary>
    /// Itemcontainer für einen Spieler in einer Spielerliste
    /// </summary>
    public class PlayerItemContainer : ItemContainer<NetworkClient>
    {
        /// <summary>
        /// Der Networkclient als Context
        /// </summary>
        public override NetworkClient DataContext { get; set; }
        public override Point Position { get; set; }
        public override Size ItemSize { get; set; }
        public SolidBrush Background { get; set; } = new SolidBrush(Color.FromArgb(175, 183, 219));
        public SolidBrush Foreground { get; set; } = new SolidBrush(Color.Black);
        public SolidBrush HighlightBackground { get; set; } = new SolidBrush(Color.FromArgb(133, 149, 222));
        public Font ForegroundFont { get; set; } = StandartText2Font;

        public override void TranslateTransform(int offX, int offY) { }
        public override void Update() { Bounds = new Rectangle(Position, ItemSize); }

        public override void Render(Graphics g)
        {
            if (IsMouseOver)
            {
                g.FillRectangle(HighlightBackground, Bounds);
            }
            else
            {
                g.FillRectangle(Background, Bounds);
            }

            Size nameSize = TextRenderer.MeasureText(DataContext.Name, ForegroundFont);
            if (ListArgs.Length > 1)
            {
                if (DataContext.Name == (ListArgs[1] as NetworkClient).Name)
                {
                    g.DrawString("♛  " + DataContext.Name, ForegroundFont, Foreground, new Point(Bounds.X + 5, Bounds.Y + (Bounds.Height / 2) - nameSize.Height / 2));
                }
                else
                {
                    g.DrawString(DataContext.Name, ForegroundFont, Foreground, new Point(Bounds.X + 5, Bounds.Y + (Bounds.Height / 2) - nameSize.Height / 2));
                }
            }
            else
            {
                g.DrawString(DataContext.Name, ForegroundFont, Foreground, new Point(Bounds.X + 5, Bounds.Y + (Bounds.Height / 2) - nameSize.Height / 2));
            }
        }
    }
}
