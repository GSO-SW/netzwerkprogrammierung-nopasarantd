using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// private protected => Nur Privater oder Vererbter instanzen wird das Verändern ermöglicht
        /// </summary>
        public bool IsMouseOver { get; private protected set; } = false;

        public GuiComponent() 
        {
            Engine.OnMouseMove += MouseMoveBase;
        }

        public void MouseMoveBase(MouseEventArgs args)
        {
            if (Bounds.Contains(args.Location))
                IsMouseOver = true;
            else
                IsMouseOver = false;
        }       
    }
}
