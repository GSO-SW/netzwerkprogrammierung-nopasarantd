using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Engine.Visuals
{
    public class GuiComponent 
    {
        /// <summary>
        /// Ist das Control sichtbar?
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Ist das Control interakionsfähig?
        /// </summary>
        public bool Active { get; set; } = true;
        /// <summary>
        /// Die Grenzen des Controls
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Befindet sich die Maus über dem Control zurzeit?
        /// </summary>
        public bool IsMouseOver { get => Bounds.Contains(Engine.MouseX, Engine.MouseY); }

		public virtual void Update() { }
		public virtual void Render(Graphics g) { }

		public virtual void KeyUp(KeyEventArgs e) { }
		public virtual void KeyDown(KeyEventArgs e) { }

		public virtual void MouseUp(MouseEventArgs e) { }
		public virtual void MouseDown(MouseEventArgs e) { }
		public virtual void MouseMove(MouseEventArgs e) { }

        public static Font StandartHeader1Font = new Font("Arial",24);
        public static Font StandartHeader2Font = new Font("Arial", 16);

    }
}
