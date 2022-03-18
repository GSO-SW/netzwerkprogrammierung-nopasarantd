using NoPasaranTD.Engine;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals
{
    public class GuiComponent : IDisposable
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
        public bool IsMouseOver { get => Bounds.Contains(StaticEngine.MouseX, StaticEngine.MouseY); }

		public virtual void Update() { }
		public virtual void Render(Graphics g) { }

		public virtual void KeyUp(KeyEventArgs e) { }
        public virtual void KeyPress(KeyPressEventArgs e) { }
        public virtual void KeyDown(KeyEventArgs e) { }

        public virtual void MouseUp(MouseEventArgs e) { }
		public virtual void MouseDown(MouseEventArgs e) { }
        public virtual void MouseMove(MouseEventArgs e) { }
        public virtual void MouseWheel(MouseEventArgs e) { }

        public virtual void Dispose() { }

        public static Font StandartIconFont = new Font("Arial", 40);
        public static Font StandartHeader1Font = new Font("Arial",24);
        public static Font StandartHeader2Font = new Font("Arial", 16,FontStyle.Bold,GraphicsUnit.Point);
        public static Font StandartText1Font = new Font("Arial", 11, FontStyle.Regular, GraphicsUnit.Point);
        public static Font StandartText2Font = new Font("Arial", 9, FontStyle.Regular, GraphicsUnit.Point);

    }
}
