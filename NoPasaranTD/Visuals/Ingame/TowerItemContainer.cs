using NoPasaranTD.Data;
using NoPasaranTD.Model;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    /// <summary>
    /// Item Container speziell für Towers Objekte
    /// </summary>
    public class TowerItemContainer : ItemContainer<Tower>
    {
        #region Public Properties

        /// <summary>
        /// Hintergrund Farbe des Items
        /// </summary>
        public Brush BackgroundBrush { get; set; } = Brushes.Thistle;
        /// <summary>
        /// Model Item
        /// </summary>
        public override Tower DataContext { get; set; }

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

        /// <summary>
        /// Content des Items
        /// </summary>
        public Image Content { get; set; }

        public SolidBrush Foreground { get; set; } = new SolidBrush(Color.FromArgb(200, 14, 14, 14));
        #endregion
        #region Private Methods

        // Prüft ob sich der Container im Sichtbaren Bereich befindet. Wenn ja dann darf dieser gezeichnet werden, wenn nicht dann nicht.
        private void PositionChanged()
            => Visible = Position.X + ItemSize.Width >= ParentBounds.X && Position.X <= ParentBounds.X + ParentBounds.Width;

        #endregion
        #region Public Methods

        public override void Render(Graphics g)
        {
            // TODO: Ausehen auf Towerart spezialisieren
            if (Visible)
            {
                if (IsMouseOver)
                    g.FillRectangle(Brushes.Red, Bounds);
                else
                    g.FillRectangle(BackgroundBrush, Bounds);
                //try { g.DrawImage(Content, Bounds.X + 3, Bounds.Y + 3, Bounds.Width - 6, Bounds.Height - 6); }
                //catch (Exception) { }

                // Die Größe des angezeigten Preises
                Size priceSize = TextRenderer.MeasureText(StaticInfo.GetTowerPrice(DataContext.GetType()).ToString(), GuiComponent.StandartHeader2Font);
                g.DrawString(StaticInfo.GetTowerPrice(DataContext.GetType()).ToString(), GuiComponent.StandartHeader2Font, Foreground, Bounds.X + Bounds.Width / 2 - priceSize.Width / 2, Bounds.Y + Bounds.Height - priceSize.Height - 5);

            }
        }

        /// <summary>
        /// Ändert die Position des ItemContainers mit einer Delta-Entfernung
        /// </summary>
        /// <param name="offX"></param>
        /// <param name="offY"></param>
        public override void TranslateTransform(int offX, int offY) =>
            Position = new Point(offX + Position.X, offY + Position.Y);
        #endregion
    }
}
