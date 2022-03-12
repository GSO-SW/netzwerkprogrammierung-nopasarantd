﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals
{
    public class TextBoxContainer : GuiComponent
    {
        public string Text { get; set; } = "";
        public int Margin { get; set; }
        public SolidBrush BorderBrush { get; set; } = new SolidBrush(Color.Gray);
        public SolidBrush Foreground { get; set; } = new SolidBrush(Color.Black);
        public SolidBrush Background { get; set; } = new SolidBrush(Color.White);
        public Font TextFont { get; set; } = StandartText2Font;
        public int CaretIndex { get; set; }
        public bool IsFocused { get; set; }
        private Rectangle innerBound => new Rectangle(Bounds.X + Margin, Bounds.Y + Margin, Bounds.Width - Margin * 2, Bounds.Height - Margin * 2);
        private float offsetX = 0;

        public override void KeyPress(KeyPressEventArgs e)
        {
            if (IsFocused)
            {
                if (e.KeyChar == '\b' && CaretIndex > 0 && Text.Length >= 0)
                {
                    string left = Text.Substring(0, CaretIndex-1);
                    string right = Text.Substring(CaretIndex, Text.Length - CaretIndex);
                    Text = left + right;
                    CaretIndex--;
                }                    
                else 
                {
                    if (e.KeyChar == '\b')
                        return;

                    if (CaretIndex == Text.Length)
                        Text += e.KeyChar;
                    else if (Text.Length > 0)
                    {
                        string left = Text.Substring(0, CaretIndex);
                        string right = Text.Substring(CaretIndex, Text.Length - CaretIndex);
                        left += e.KeyChar;
                        Text = left + right;
                    }
                    CaretIndex++;
                }                
            }
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && CaretIndex > 0)
                CaretIndex--;
            else if (e.KeyCode == Keys.Right && CaretIndex <= Text.Length - 1)
                CaretIndex++;
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

            Matrix current = g.Transform;
            Region currentClip = g.Clip;

            g.SetClip(innerBound);
            g.TranslateTransform(offsetX, 0);                      
            g.DrawString(Text, TextFont,Foreground,innerBound.X, innerBound.Y,new StringFormat(StringFormatFlags.NoWrap));
            g.Clip = currentClip;

            g.Transform = current;

            if (Text != "" && IsFocused)
            {
                SizeF leftTextSize = g.MeasureString(Text.Substring(0, CaretIndex), TextFont);
                g.DrawLine(new Pen(Foreground), Bounds.X + leftTextSize.Width, Bounds.Y + 1, Bounds.X + leftTextSize.Width, Bounds.Y + leftTextSize.Height - 2);

                if (leftTextSize.Width >= innerBound.Width)
                    offsetX = innerBound.Width - leftTextSize.Width;
            }
            else if (IsFocused)
                g.DrawLine(new Pen(Foreground), Bounds.X + 2, Bounds.Y + 1, Bounds.X + 2, Bounds.Y + 10);
                                    
        }
    }
}
