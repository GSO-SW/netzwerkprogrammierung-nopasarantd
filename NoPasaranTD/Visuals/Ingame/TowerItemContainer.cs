using NoPasaranTD.Data;
using NoPasaranTD.Model;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        public Brush BackgroundBrush { get; set; } = new SolidBrush(Color.FromArgb(90, 112, 191));

        private Tower dataContext;
        /// <summary>
        /// Model Item
        /// </summary>
        public override Tower DataContext
        {
            get => dataContext;

            set
            {
                dataContext = value;
                DataContext.IsSelected = false;
                DataContext.IsPlaced = true;
                Size size = StaticInfo.GetTowerSize(dataContext.GetType());
                DataContext.Hitbox = new Rectangle(new Point(-size.Width / 2, -size.Height / 2), size); // Größe des Turmes speichern
            }
        }

        /// <summary>
        /// Position des Item-Containers auf dem Screen
        /// </summary>
        public override Point Position
        {
            get => new Point(Bounds.X, Bounds.Y);
            set { Bounds = new Rectangle(value, ItemSize); PositionChanged(); }
        }

        /// <summary>
        /// Größe des Items
        /// </summary>
        public override Size ItemSize
        {
            get => new Size(Bounds.Width, Bounds.Height);
            set => Bounds = new Rectangle(Position, value);
        }

        private SizeF scaleFactor = new SizeF(1, 1); // Faktor zum anpassen der angezeigten Türme

        public SolidBrush Foreground { get; set; } = new SolidBrush(Color.FromArgb(200, 14, 14, 14));
        #endregion
        #region Private Methods

        // Prüft ob sich der Container im Sichtbaren Bereich befindet. Wenn ja dann darf dieser gezeichnet werden, wenn nicht dann nicht.
        private void PositionChanged()
        {
            Visible = Position.X + ItemSize.Width >= ParentBounds.X && Position.X <= ParentBounds.X + ParentBounds.Width;
        }

        #endregion
        #region Public Methods

        public override void Render(Graphics g)
        {
            if (Visible)
            {
                if (IsMouseOver) // Der Spieler Hovert mit der Maus über dem Container
                {
                    g.FillRectangle(Brushes.SlateGray, Bounds);
                }
                else
                {
                    g.FillRectangle(BackgroundBrush, Bounds);
                }

                Matrix matrix = g.Transform;
                g.TranslateTransform(Position.X + ItemSize.Width / 2, Position.Y + ItemSize.Height / 2); // Neuen Nullpunkt setzen
                // Faktor zum skalieren berechnen
                scaleFactor = new SizeF((float)Math.Min(dataContext.Hitbox.Width, ItemSize.Width) / Math.Max(dataContext.Hitbox.Width, ItemSize.Width), (float)Math.Min(dataContext.Hitbox.Width, ItemSize.Width) / Math.Max(dataContext.Hitbox.Width, ItemSize.Width));
                g.ScaleTransform(scaleFactor.Width, scaleFactor.Height);
                DataContext.Render(g);
                g.Transform = matrix; // Transformation zurück wandeln

                // Die Größe des angezeigten Preises
                Size priceSize = TextRenderer.MeasureText(StaticInfo.GetTowerPrice(DataContext.GetType()).ToString() + " ₿", GuiComponent.StandartHeader2Font);
                g.DrawString(StaticInfo.GetTowerPrice(DataContext.GetType()).ToString() + " ₿", GuiComponent.StandartHeader2Font, Foreground, Bounds.X + Bounds.Width / 2 - priceSize.Width / 2, Bounds.Y + Bounds.Height - priceSize.Height - 5);

            }
        }

        /// <summary>
        /// Ändert die Position des ItemContainers mit einer Delta-Entfernung
        /// </summary>
        /// <param name="offX"></param>
        /// <param name="offY"></param>
        public override void TranslateTransform(int offX, int offY)
        {
            Position = new Point(offX + Position.X, offY + Position.Y);
        }
        #endregion
    }
}
