﻿using NoPasaranTD.Data;
using NoPasaranTD.Model;
using System;
using System.Drawing;
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

        #region Render region
        private void Display_Paint(object sender, PaintEventArgs e)
        {
            float scaledWidth = (float)ClientSize.Width / Engine.RenderWidth;
            float scaledHeight = (float)ClientSize.Height / Engine.RenderHeight;

            Graphics g = e.Graphics;
            g.ScaleTransform(scaledWidth, scaledHeight);
            { // Spiel rendern
                currentGame.Render(g);
            }
            g.ResetTransform();
        }

        private void tmrGameUpdate_Tick(object sender, EventArgs e)
        {
            Engine.Update();
            while (Engine.ElapsedTicks > 0)
            {
                currentGame.Update();
                Engine.ElapsedTicks--;
            }

            { // Framerate aktualisieren (falls geändert)
                int fps = Math.Max(1, 1000 / Engine.Framerate);
                if (tmrGameUpdate.Interval != fps)
                    tmrGameUpdate.Interval = fps;
            }

            Refresh();
        }
        #endregion

    }
}
