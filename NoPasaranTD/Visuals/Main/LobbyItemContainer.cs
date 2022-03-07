using System.Drawing;

namespace NoPasaranTD.Visuals.Main
{
    // TODO: Objekt zu Lobby ändern
    public class LobbyItemContainer : ItemContainer<object>
    {
        #region Property region
        private float borderWidth;
        private Pen borderPen;
        private Pen hoverPen;

        /// <summary>
        /// Hintergrundfarbe des Containers
        /// </summary>
        public Brush BackgroundBrush { get; set; }

        /// <summary>
        /// Vordergrundfarbe des Containers
        /// </summary>
        public Brush ForegroundBrush { get; set; }

        /// <summary>
        /// Randfarbe des Containers
        /// </summary>
        public Color BorderColor
        {
            set
            {
                borderPen = new Pen(value, borderWidth);
                hoverPen = new Pen(Color.FromArgb(
                    (int)(value.R / 1.5f),
                    (int)(value.G / 1.5f),
                    (int)(value.B / 1.5f)
                ), borderWidth);
            }
        }

        /// <summary>
        /// Breite des Container-Randes
        /// </summary>
        public float BorderWidth
        {
            get { return borderWidth; }
            set
            {
                borderWidth = value;
                if (borderPen != null) borderPen.Width = value;
                if (hoverPen != null) hoverPen.Width = value;
            }
        }
        #endregion

        #region Implementation region
        /// <summary>
        /// Model Item
        /// </summary>
        public override object DataContext { get; set; }

        /// <summary>
        /// Position des Item-Containers auf dem Screen
        /// </summary>
        public override Point Position 
        { 
            get { return Bounds.Location; }
            set { Bounds = new Rectangle(value, Bounds.Size); PositionChanged(); }
        }

        /// <summary>
        /// Größe des Items
        /// </summary>
        public override Size ItemSize 
        { 
            get { return Bounds.Size; }
            set { Bounds = new Rectangle(Bounds.Location, value); }
        }

        /// <summary>
        /// Ändert die Position des ItemContainers mit einer Delta-Entfernung
        /// </summary>
        /// <param name="offX">Entfernung der X-Position</param>
        /// <param name="offY">Entfernung der Y-Position</param>
        public override void TranslateTransform(int offX, int offY)
            => Position += new Size(offX, offY);

        // Prüft ob sich der Container im sichtbaren Bereich befindet. Wenn ja, dann darf dieser gezeichnet werden. Wenn nicht, dann nicht.
        private void PositionChanged()
            => Visible = Position.Y + ItemSize.Height >= ParentBounds.Y && Position.Y <= ParentBounds.Y + ParentBounds.Height;
        #endregion

        public LobbyItemContainer()
        {
            BackgroundBrush = Brushes.LightGray;
            ForegroundBrush = Brushes.Black;
            BorderColor = Color.Blue;
            BorderWidth = 2f;
        }

        public override void Render(Graphics g)
        {
            if (!Visible) return;

            g.FillRectangle(BackgroundBrush, Bounds);
            g.DrawRectangle(IsMouseOver ? hoverPen : borderPen, Bounds);

            // TODO: Rendern der Lobby informationen
            g.DrawString(DataContext.ToString(), StandartHeader1Font, ForegroundBrush, Bounds);
        }
    }
}
