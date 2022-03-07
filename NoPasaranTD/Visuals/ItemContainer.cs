using System.Drawing;

namespace NoPasaranTD.Visuals
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
}
