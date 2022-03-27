using NoPasaranTD.Data;
using NoPasaranTD.Model;
using NoPasaranTD.Networking;
using NoPasaranTD.Visuals;
using NoPasaranTD.Visuals.Main;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Engine
{
    public partial class StaticDisplay : Form
    {

        private GuiComponent currentScreen;
        private Game currentGame;

        public StaticDisplay()
        {
            InitializeComponent();
        }

        private void Display_FormClosing(object sender, FormClosingEventArgs e)
        {
            Game backupGame = currentGame; // Zwischengespeichert für Fehlervorbeugung beim Aktualisieren
            currentGame = null;
            backupGame?.Dispose(); // Game instanz freigeben

            GuiComponent backupScreen = currentScreen; // Zwischengespeichert für Fehlervorbeugung beim Aktualisieren
            currentScreen = null;
            backupScreen?.Dispose(); // Screen instanz freigeben
        }

        /// <summary>
        /// Lade Spielinstanz im Offlinemodus.<br/>
        /// Falls der Parameter eine Null-Referenz ist, 
        /// entlädt er das Spiel und kehrt automatisch ins Hauptmenü zurück
        /// </summary>
        /// <param name="mapFile">Dateiname der Map</param>
        public void LoadGame(string mapFile)
        {
            LoadGame(mapFile, mapFile == null ? null : new NetworkHandler());
        }

        /// <summary>
        /// Lade Spielinstanz im Onlinemodus.<br/>
        /// Falls der Parameter eine Null-Referenz ist, 
        /// entlädt er das Spiel und kehrt automatisch ins Hauptmenü zurück
        /// </summary>
        /// <param name="mapFile">Dateiname der Map</param>
        /// <param name="handler">Dementsprechender Netzwerkmanager</param>
        public void LoadGame(string mapFile, NetworkHandler handler)
        {
            Game backupGame = currentGame; // Zwischengespeichert für Fehlervorbeugung beim Aktualisieren

            if (mapFile == null)
            {
                currentGame = null; // Entlade Spiel
                GuiStartMenü startMenü = new GuiStartMenü();
                startMenü.Init(this);
                LoadScreen(startMenü); // Lade Hauptmenu
            }
            else
            {
                // Lade map und initialisiere sie
                Map map = MapData.GetMapByFileName(mapFile); map.Initialize();
                currentGame = new Game(map, handler);
                LoadScreen(null); // Screen entladen
            }

            backupGame?.Dispose(); // Game instanz freigeben
        }

        /// <summary>
        /// Lade einen überlappenden Screen<br/>
        /// Achtung: Das Spiel wird dabei nicht automatisch gestoppt, 
        /// sondern läuft im Hintergrund weiter!
        /// </summary>
        /// <param name="screen">Der zu ladende Screen</param>
        public void LoadScreen(GuiComponent screen)
        {
            GuiComponent backupScreen = currentScreen; // Zwischengespeichert für Fehlervorbeugung beim Aktualisieren
            currentScreen = screen;
            backupScreen?.Dispose();
        }

        #region Mouse region
        /// <summary>
        /// Berechne die Mausposition auf dem Bildschirm und führe alle registrierten Events aus
        /// </summary>
        private void Display_MouseUp(object sender, MouseEventArgs e)
        {
            // x & y zwischenspeichern, augfrund von anderen Events,
            // die Engine.MouseX und Engine.MouseY ändern könnten
            int x = (int)((float)StaticEngine.RenderWidth / ClientSize.Width * e.X);
            int y = (int)((float)StaticEngine.RenderHeight / ClientSize.Height * e.Y);

            StaticEngine.MouseX = x;
            StaticEngine.MouseY = y;

            MouseEventArgs args = new MouseEventArgs(
                e.Button, e.Clicks, x, y, e.Delta
            );

            currentGame?.MouseUp(args);
            currentScreen?.MouseUp(args);
        }

        /// <summary>
        /// Berechne die Mausposition auf dem Bildschirm und führe alle registrierten Events aus
        /// </summary>
        private void Display_MouseDown(object sender, MouseEventArgs e)
        {
            // x & y zwischenspeichern, augfrund von anderen Events,
            // die Engine.MouseX und Engine.MouseY ändern könnten
            int x = (int)((float)StaticEngine.RenderWidth / ClientSize.Width * e.X);
            int y = (int)((float)StaticEngine.RenderHeight / ClientSize.Height * e.Y);

            StaticEngine.MouseX = x;
            StaticEngine.MouseY = y;

            MouseEventArgs args = new MouseEventArgs(
                e.Button, e.Clicks, x, y, e.Delta
            );

            currentGame?.MouseDown(args);
            currentScreen?.MouseDown(args);
        }

        /// <summary>
        /// Berechne die Mausposition auf dem Bildschirm und führe alle registrierten Events aus
        /// </summary>
        private void Display_MouseMove(object sender, MouseEventArgs e)
        {
            // x & y zwischenspeichern, augfrund von anderen Events,
            // die Engine.MouseX und Engine.MouseY ändern könnten
            int x = (int)((float)StaticEngine.RenderWidth / ClientSize.Width * e.X);
            int y = (int)((float)StaticEngine.RenderHeight / ClientSize.Height * e.Y);

            StaticEngine.MouseX = x;
            StaticEngine.MouseY = y;

            MouseEventArgs args = new MouseEventArgs(
                e.Button, e.Clicks, x, y, e.Delta
            );

            currentGame?.MouseMove(args);
            currentScreen?.MouseMove(args);
        }

        /// <summary>
        /// Berechne die Mausposition auf dem Bildschirm und führe alle registrierten Events aus
        /// </summary>
        private void Display_MouseWheel(object sender, MouseEventArgs e)
        {
            // x & y zwischenspeichern, augfrund von anderen Events,
            // die Engine.MouseX und Engine.MouseY ändern könnten
            int x = (int)((float)StaticEngine.RenderWidth / ClientSize.Width * e.X);
            int y = (int)((float)StaticEngine.RenderHeight / ClientSize.Height * e.Y);

            StaticEngine.MouseX = x;
            StaticEngine.MouseY = y;

            MouseEventArgs args = new MouseEventArgs(
                e.Button, e.Clicks, x, y, e.Delta
            );

            currentGame?.MouseWheel(args);
            currentScreen?.MouseWheel(args);
        }
        #endregion

        #region Keyboard region
        private void Display_KeyUp(object sender, KeyEventArgs e)
        {
            currentGame?.KeyUp(e);
            currentScreen?.KeyUp(e);
        }

        private void Display_KeyPress(object sender, KeyPressEventArgs e)
        {
            currentGame?.KeyPress(e);
            currentScreen?.KeyPress(e);
        }

        private void Display_KeyDown(object sender, KeyEventArgs e)
        {
            currentGame?.KeyDown(e);
            currentScreen?.KeyDown(e);
        }
        #endregion

        #region Render region
        private void Display_Paint(object sender, PaintEventArgs e)
        {
            float scaledWidth = (float)ClientSize.Width / StaticEngine.RenderWidth;
            float scaledHeight = (float)ClientSize.Height / StaticEngine.RenderHeight;

            Graphics g = e.Graphics;
            g.ScaleTransform(scaledWidth, scaledHeight);
            { // Spiel rendern
                currentGame?.Render(g);
                currentScreen?.Render(g);
            }
            g.ResetTransform();
        }

        private void tmrGameUpdate_Tick(object sender, EventArgs e)
        {
            StaticEngine.Update();
            while (StaticEngine.ElapsedTicks > 0)
            {
                currentGame?.Update();
                currentScreen?.Update();
                StaticEngine.ElapsedTicks--;
            }

            { // Framerate aktualisieren (falls geändert)
                int fps = Math.Max(1, 1000 / StaticEngine.Framerate);
                if (tmrGameUpdate.Interval != fps)
                {
                    tmrGameUpdate.Interval = fps;
                }
            }

            Refresh();
        }
        #endregion

    }
}
