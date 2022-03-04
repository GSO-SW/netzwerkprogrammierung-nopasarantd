using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Engine.Visuals
{
    /// <summary>
    /// Basisklasse für Item Containers eines List Containers
    /// </summary>
    /// <typeparam name="T">Container Typ z.b TowerItemContainer</typeparam>
    public abstract class ItemContainer<T> : GuiComponent
    {
        /// <summary>
        /// Der Model Context des Item Containers
        /// </summary>
        public abstract T DataContext { get; set; }
        /// <summary>
        /// Die Position des Item Containers auf dem Bildschirm
        /// </summary>
        public abstract Point Position { get; set; }

        /// <summary>
        /// Die Größe des Item-Containers
        /// </summary>
        public abstract Size ItemSize { get; set; }

        public Rectangle ParentBounds { get; set; }  

        public abstract void TranslateTransform(int offX, int offY);
    }

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

        #endregion
        #region Private Methods

        // Prüft ob sich der Container im Sichtbaren Bereich befindet. Wenn ja dann darf dieser gezeichnet werden, wenn nicht dann nicht.
        private void PositionChanged()
            => Visible = Position.X >= ParentBounds.X && Position.X + ItemSize.Width <= ParentBounds.X + ParentBounds.Width;

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
