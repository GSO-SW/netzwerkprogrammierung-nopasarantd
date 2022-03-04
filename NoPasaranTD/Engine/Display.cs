using NoPasaranTD.Data;
using NoPasaranTD.Model;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace NoPasaranTD.Engine
{
    public partial class Display : Form
    {

        private Game currentGame;
        public Display()
        {
            InitializeComponent();
            LoadDefaultGame();
        }

        private void LoadDefaultGame()
        {
            Map map = MapData.GetMapByFile("test2");
            map.Initialize();
            currentGame = new Game(map);
        }

        private void Display_Load(object sender, EventArgs e)
            => new Thread(GameLoop).Start();

        #region Mouse region
        /// <summary>
        /// Berechne die Mausposition auf dem Bildschirm und führe alle registrierten Events aus
        /// </summary>
        private void Display_MouseUp(object sender, MouseEventArgs e)
        {
            // x & y zwischenspeichern, augfrund von anderen Events,
            // die Engine.MouseX und Engine.MouseY ändern könnten
            int x = (int)((float)Engine.RenderWidth / ClientSize.Width * e.X);
            int y = (int)((float)Engine.RenderHeight / ClientSize.Height * e.Y);

            Engine.MouseX = x;
            Engine.MouseY = y;

            currentGame.MouseUp(new MouseEventArgs(
                e.Button, e.Clicks, x, y, e.Delta
            ));
        }

        /// <summary>
        /// Berechne die Mausposition auf dem Bildschirm und führe alle registrierten Events aus
        /// </summary>
        private void Display_MouseDown(object sender, MouseEventArgs e)
        {
            // x & y zwischenspeichern, augfrund von anderen Events,
            // die Engine.MouseX und Engine.MouseY ändern könnten
            int x = (int)((float)Engine.RenderWidth / ClientSize.Width * e.X);
            int y = (int)((float)Engine.RenderHeight / ClientSize.Height * e.Y);

            Engine.MouseX = x;
            Engine.MouseY = y;

            currentGame.MouseDown(new MouseEventArgs(
                e.Button, e.Clicks, x, y, e.Delta
            ));
        }

        /// <summary>
        /// Berechne die Mausposition auf dem Bildschirm und führe alle registrierten Events aus
        /// </summary>
        private void Display_MouseMove(object sender, MouseEventArgs e)
        {
            // x & y zwischenspeichern, augfrund von anderen Events,
            // die Engine.MouseX und Engine.MouseY ändern könnten
            int x = (int)((float)Engine.RenderWidth / ClientSize.Width * e.X);
            int y = (int)((float)Engine.RenderHeight / ClientSize.Height * e.Y);

            Engine.MouseX = x;
            Engine.MouseY = y;

            currentGame.MouseMove(new MouseEventArgs(
                e.Button, e.Clicks, x, y, e.Delta
            ));
        }

        /// <summary>
        /// Berechne die Mausposition auf dem Bildschirm und führe alle registrierten Events aus
        /// </summary>
        private void Display_MouseWheel(object sender, MouseEventArgs e)
        {
            // x & y zwischenspeichern, augfrund von anderen Events,
            // die Engine.MouseX und Engine.MouseY ändern könnten
            int x = (int)((float)Engine.RenderWidth / ClientSize.Width * e.X);
            int y = (int)((float)Engine.RenderHeight / ClientSize.Height * e.Y);

            Engine.MouseX = x;
            Engine.MouseY = y;

            currentGame.MouseWheel(new MouseEventArgs(
                e.Button, e.Clicks, x, y, e.Delta
            ));
        }
        #endregion

        #region Keyboard region
        private void Display_KeyUp(object sender, KeyEventArgs e) => currentGame.KeyUp(e);
        private void Display_KeyDown(object sender, KeyEventArgs e) => currentGame.KeyDown(e);
        #endregion

        private void Display_Paint(object sender, PaintEventArgs e)
        {
            float scaledWidth = (float)ClientSize.Width / Engine.RenderBuffer.Width;
            float scaledHeight = (float)ClientSize.Height / Engine.RenderBuffer.Height;

            Graphics g = e.Graphics;
            g.ScaleTransform(scaledWidth, scaledHeight);
            g.DrawImageUnscaled(Engine.RenderBuffer, 0, 0);
            g.ResetTransform();
        }

        private void GameLoop()
        {
            ulong ticksUnhandled = 0;

            Func<bool> focusAction = () => Focused;
            Action refreshAction = () => Refresh();

            int lastTick = Environment.TickCount;
            while (Visible)
            {
                // TODO: Ändern zu jetzige Server-Ticks
                int currTick = Environment.TickCount;
                int deltaTick = currTick - lastTick;
                ticksUnhandled += (ulong)deltaTick;
                lastTick = currTick;

                while (ticksUnhandled > 0)
                {
                    currentGame.Update();
                    ticksUnhandled --;
                }

                try
                { // TODO: Fehlerfrei und threadübergreiffend aktualisieren
                    if ((bool)Invoke(focusAction))
                    {
                        Engine.RenderGraphics.Clear(Color.White);
                        Engine.RenderGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                        currentGame.Render(Engine.RenderGraphics);
                        Invoke(refreshAction);
                    }
                }
                catch (Exception) { break; }

                Engine.Sync();
            }
        }
    }
}
