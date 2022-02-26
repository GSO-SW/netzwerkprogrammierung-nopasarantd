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
        public bool Visible { get; set; } = true;
        public bool Active { get; set; } = true;
        public Rectangle Bounds { get; set; }
        public bool IsMouseOver { get; set; } = false;

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
