using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals
{
    public class TextBoxContainer : GuiComponent
    {
        public string Text { get; set; } = "\0";
        public int Margin { get; set; }
        public SolidBrush BorderBrush { get; set; } = new SolidBrush(Color.Gray);
        public SolidBrush Foreground { get; set; } = new SolidBrush(Color.Black);
        public SolidBrush Background { get; set; } = new SolidBrush(Color.White);
        public Font TextFont { get; set; } = StandartText2Font;
        public int CaretIndex { get; set; }
        public bool IsFocused { get; set; }

        private KeysConverter keyConverter = new KeysConverter();
        private Rectangle innerBound => new Rectangle(Bounds.X + Margin, Bounds.Y + Margin, Bounds.Width - Margin * 2, Bounds.Height - Margin * 2);

        public override void KeyDown(KeyEventArgs e)
        {
            if (IsFocused)
            {
                if ((char)e.KeyCode == '\b')
                    Text = Text.Substring(0, Text.Length - 1);
                else
                    Text += (char)e.KeyValue;

                CaretIndex = Text.Length - 1;
                if (CaretIndex == 0)
                    CaretIndex = 1;
            }            
        }

        public override void MouseDown(MouseEventArgs e)
        {
            if (Bounds.Contains(e.Location))
                IsFocused = true;
            else
                IsFocused = false;
        }

        public override void Render(Graphics g)
        {
            g.FillRectangle(BorderBrush,Bounds);
            g.FillRectangle(Background, innerBound);
            g.DrawString(Text, TextFont,Foreground,innerBound);
          
            Size leftTextSize = TextRenderer.MeasureText(Text.Substring(0,CaretIndex),TextFont);

            if (!(leftTextSize == Size.Empty))
                g.DrawLine(new Pen(Foreground), Bounds.X + leftTextSize.Width + 3, Bounds.Y + 1, Bounds.X + leftTextSize.Width + 3, Bounds.Y + leftTextSize.Height - 2);
            
        }
    }
}
