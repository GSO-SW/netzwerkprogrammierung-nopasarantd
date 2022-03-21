using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals
{
    /// <summary>
    /// Ein Controlelemnt zur Bestätigung (Ein Button eben...)
    /// </summary>
    public class ButtonContainer : GuiComponent
    {
        public delegate void ButtonClickedEventHandler();

        // Event welches beim Betätigen des Buttons ausgelößt wird
        public event ButtonClickedEventHandler ButtonClicked;

        /// <summary>
        /// Das Element welches auf dem Button angezeigt werden soll
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// Die Font des Button Textes
        /// </summary>
        public Font StringFont { get; set; }

        /// <summary>
        /// Die Schriftfarbe des Buttontextes
        /// </summary>
        public Brush Foreground { get; set; }

        /// <summary>
        /// Die Hintergrundfarbe des Buttons
        /// </summary>
        public Brush Background { get; set; }

        /// <summary>
        /// Die Randfarbe des Buttons
        /// </summary>
        public Brush BorderBrush { get; set; }

        /// <summary>
        /// Die Randgröße des Buttons
        /// </summary>
        public int Margin { get; set; } = 0;

        // Das innere Rechteck eines Buttons
        private Rectangle innerBackground;

        public ButtonContainer(Rectangle bounds,int margin,string content)
        {
            Content = content;
            Margin = margin;
            Bounds = bounds;
        }

        public ButtonContainer() { }

        public override void MouseDown(MouseEventArgs e)
        {
            if (!Visible) return;
            // Löst das Button Clicked Event aus wenn innerhalb der Bounds gecklickt wird
            if (Bounds.Contains(e.Location))
                ButtonClicked?.Invoke();
        }
        
        public override void Render(Graphics g)
        {
            if (!Visible) return;
            innerBackground = new Rectangle(Bounds.X + Margin, Bounds.Y + Margin, Bounds.Width - Margin * 2, Bounds.Height - Margin * 2);
            
            // Highlightet den Rand des Buttons wenn die Maus drüber erscheint
            if (!IsMouseOver)
                g.FillRectangle(BorderBrush, Bounds);
            else
            {
                // Normaler Zustand wenn die Maus nicht über dem Button liegt
                Color borderColor = (BorderBrush as SolidBrush).Color;
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(borderColor.R / 1.5), (int)(borderColor.G / 1.5), (int)(borderColor.B / 1.5))), Bounds);
            }
               
            // Füllt den Hintergrund des Buttons
            g.FillRectangle(Background, innerBackground);

            // Zeichnet einen Text wenn die Content Property ein String ist
            if (Content is string)
            {
                Size fontSize = TextRenderer.MeasureText(Content as string, StringFont);
                g.DrawString(Content as string, StringFont, Foreground, new PointF(innerBackground.X + innerBackground.Width / 2 - fontSize.Width / 2, innerBackground.Y + innerBackground.Height / 2 - fontSize.Height / 2));
            }
            else if (Content is Image) // Zeichnet ein Bild wenn die Content Property ein Image ist
            {
                // TODO: Image Button
            }
        }
    }
}
