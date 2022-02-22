using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Engine.Visuals
{
    /// <summary>
    /// Basisklasse für Item Containers eines List Containers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ItemContainer<T>
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

        /// <summary>
        /// Befindet sich der Cursor zurzeit auf dem Item Container
        /// </summary>
        public bool IsMouseOver { get; protected private set; } = false;
    }

    /// <summary>
    /// Item Container speziell für Towers Objekte
    /// </summary>
    public class TowerItemContainer : ItemContainer<Tower>
    {
        #region Constructor

        public TowerItemContainer()
        {
            Engine.OnRender += DrawItem;
            Engine.OnMouseMove += MouseMove; 
            Content = Image.FromFile(Environment.CurrentDirectory + "\\img\\monkey.jpg");
        }

        #endregion
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
            get { return new Point(background.X, background.Y); } 
            set { background.X = value.X; background.Y = value.Y; } 
        }

        /// <summary>
        /// Größe des Items
        /// </summary>
        public override Size ItemSize 
        { 
            get { return new Size(background.Width, background.Height); } 
            set { background.Width = value.Width; background.Height = value.Height; } 
        }

        /// <summary>
        /// Content des Items
        /// </summary>
        public Image Content { get; set; }

        /// <summary>
        /// Ist der Container Sichtbar oder nicht
        /// </summary>
        public bool IsOpen
        {
            set
            {
                if (!value)
                    Engine.OnRender -= DrawItem;
                else
                    Engine.OnRender += DrawItem;
            }
        }

        #endregion
        #region Private Members

        private Rectangle background = new Rectangle(); // Hintergrund des Item Containers

        #endregion
        #region Private Methodes

        private void DrawItem(Graphics g)
        {
            if (IsMouseOver)
                g.FillRectangle(Brushes.Red, background);
            else
                g.FillRectangle(BackgroundBrush, background);

            g.DrawImage(Content,background.X + 3, background.Y + 3, background.Width - 6, background.Height -6);            
        }

        private void MouseMove(MouseEventArgs args)
        {
            if (background.Contains(new Point(Engine.MouseX, Engine.MouseY)))
                IsMouseOver = true;
            else
                IsMouseOver = false;
        }

        #endregion
    }
}
