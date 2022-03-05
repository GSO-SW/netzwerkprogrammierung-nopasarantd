using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Engine.Visuals
{
    public class ButtonContainer : GuiComponent
    {
        public delegate void ButtonClickedEventHandler();
        public event ButtonClickedEventHandler ButtonClicked;

        public object Content { get; set; }
        public Font StringFont { get; set; }
        public Brush Foreground { get; set; }
        public Brush Background { get; set; }
        public Brush BorderBrush { get; set; }
        public int Margin { get; set; } = 0;

        // Das innere Rechteck eines Buttons
        private Rectangle innerBackground;

        public ButtonContainer(Rectangle bounds,int margin,string content)
        {
            Content = content;
            Margin = margin;
            Bounds = bounds;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            if (Bounds.Contains(e.Location))
                ButtonClicked?.Invoke();
        }
        
        public override void Render(Graphics g)
        {            
            innerBackground = new Rectangle(Bounds.X+Margin,Bounds.Y+Margin,Bounds.Width-Margin*2,Bounds.Height-Margin*2);
            
            if (!IsMouseOver)
                g.FillRectangle(BorderBrush, Bounds);
            else
            {
                Color borderColor = (BorderBrush as SolidBrush).Color;
                g.FillRectangle(new SolidBrush(Color.FromArgb(255,borderColor.R/2,borderColor.G/2,borderColor.B/2)), Bounds);
            }
                
            g.FillRectangle(Background, innerBackground);

            // Zeichnet einen Text wenn die Content Property ein String ist
            if (Content is string)
            {
                Size fontSize = TextRenderer.MeasureText(Content as string, StringFont);
                g.DrawString(Content as string, StringFont, Foreground, new PointF(Bounds.X + Bounds.Width / 2 - fontSize.Width / 2, Bounds.Y + Bounds.Height / 2 - fontSize.Height / 2));
            }
            else if (Content is Image) // Zeichnet ein Bild wenn die Content Property ein Image ist
            {
                // TODO: Image Button
            }
        }
    }
}
