﻿using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals
{
    public class TextBoxContainer : GuiComponent
    {
        public string Text { get; set; } = "";
        public int Margin { get; set; }

        public SolidBrush HighlightedBorderBrush { get; set; } = new SolidBrush(Color.DodgerBlue);
        public SolidBrush BorderBrush { get; set; } = new SolidBrush(Color.Gray);
        public SolidBrush Foreground { get; set; } = new SolidBrush(Color.Black);
        public SolidBrush Background { get; set; } = new SolidBrush(Color.White);
        public Font TextFont { get; set; } = StandartText2Font;

        /// <summary>
        /// Die Position an dem das Caret stehen soll (Die Cursorposition in der Textbox)
        /// </summary>
        public int CaretIndex { get; set; }
        public bool IsFocused { get; set; }

        private Rectangle innerBound => new Rectangle(Bounds.X + Margin, Bounds.Y + Margin, Bounds.Width - Margin * 2, Bounds.Height - Margin * 2);
        private float offsetX = 0;

        public override void KeyPress(KeyPressEventArgs e)
        {
            if (IsFocused)
            {
                if (e.KeyChar == '\b' && CaretIndex > 0 && Text.Length >= 0) // Backspace 
                {
                    string left = Text.Substring(0, CaretIndex - 1);
                    string right = Text.Substring(CaretIndex, Text.Length - CaretIndex);
                    Text = left + right;
                    CaretIndex--;
                }
                else if (e.KeyChar == (char)Keys.Enter || e.KeyChar == '\n') // Sobald ein Enter-Char eingegeben wird, wird nichts eingetragen
                {
                    return;
                }
                else
                {
                    // Nur erlaubte Zeichen durch lassen
                    if (e.KeyChar < 32 || e.KeyChar > 126)
                    {
                        return;
                    }

                    if (CaretIndex == Text.Length)
                    {
                        Text += e.KeyChar;
                    }
                    else if (Text.Length > 0)
                    {
                        string left = Text.Substring(0, CaretIndex) + e.KeyChar;
                        string right = Text.Substring(CaretIndex, Text.Length - CaretIndex);
                        Text = left + right;
                    }
                    CaretIndex++;
                }
            }
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && CaretIndex > 0)
            {
                CaretIndex--;
            }
            else if (e.KeyCode == Keys.Right && CaretIndex < Text.Length)
            {
                CaretIndex++;
            }
        }

        public override void MouseDown(MouseEventArgs e)
        {
            if (Bounds.Contains(e.Location))
            {
                if (!IsFocused)
                {
                    CaretIndex = Text.Length;
                }

                IsFocused = true;
            }
            else
            {
                IsFocused = false;
            }
        }

        public override void Render(Graphics g)
        {
            g.FillRectangle(IsFocused ? HighlightedBorderBrush : BorderBrush, Bounds);
            g.FillRectangle(Background, innerBound);

            Matrix currentTransform = g.Transform;
            Region currentClip = g.Clip;

            g.SetClip(innerBound);
            g.TranslateTransform(offsetX, 0);
            g.DrawString(Text, TextFont, Foreground, innerBound.Location);

            if (!string.IsNullOrEmpty(Text) && IsFocused)
            {
                SizeF leftTextSize = g.MeasureString(Text.Substring(0, CaretIndex) + '_', TextFont);
                leftTextSize.Width -= g.MeasureString("_", TextFont).Width;

                g.DrawLine(new Pen(Foreground),
                    Bounds.X + leftTextSize.Width + 4, Bounds.Y + 1,
                    Bounds.X + leftTextSize.Width + 4, Bounds.Y + leftTextSize.Height - 2);

                if (leftTextSize.Width >= innerBound.Width)
                {
                    offsetX = innerBound.Width - leftTextSize.Width;
                }
            }
            else if (IsFocused)
            {
                g.DrawLine(new Pen(Foreground), Bounds.X + 2, Bounds.Y + 1, Bounds.X + 2, Bounds.Y + 15);
            }

            g.Clip = currentClip;
            g.Transform = currentTransform;
        }
    }
}
