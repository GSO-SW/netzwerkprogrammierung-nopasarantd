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
            itself = this; InitializeComponent();
        }
             
        public static StaticDisplay itself = null;
        private void Display_FormClosing(object sender, FormClosingEventArgs e)
        {
            itself = null;
            // Game instanz freigeben
            currentGame?.Dispose();
            currentGame = null;

            // Screen instanz freigeben
            currentScreen?.Dispose();
            currentScreen = null;
        }

        /// <summary>
        /// Lade Spielinstanz im Offlinemodus
        /// </summary>
        /// <param name="mapFile">Dateiname der Map</param>
        public void LoadGame(string mapFile) => LoadGame(mapFile, mapFile == null ? null : new NetworkHandler());

        /// <summary>
        /// Lade Spielinstanz im Onlinemodus
        /// </summary>
        /// <param name="mapFile">Dateiname der Map</param>
        /// <param name="handler">Dementsprechender Netzwerkmanager</param>
        public void LoadGame(string mapFile, NetworkHandler handler)
        {
            currentGame?.Dispose();

            if (mapFile == null)
            {
                currentGame = null; // Entlade Spiel
                LoadScreen(new GuiMainMenu()); // Lade Hauptmenu
            }
            else
            {
                // Lade map und initialisiere sie
                Map map = MapData.GetMapByFileName(mapFile); map.Initialize();
                currentGame = new Game(map, handler, this);
                LoadScreen(null); // Screen entladen
            }
        }

        public void LoadScreen(GuiComponent screen)
        {
            currentScreen?.Dispose();
            currentScreen = screen;
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

        int x;
        private void tmrGameUpdate_Tick(object sender, EventArgs e)
        {
            StaticEngine.Update();
            while (StaticEngine.ElapsedTicks > 0)
            {
                x++; 
                currentGame?.Update();
                currentScreen?.Update();
                StaticEngine.ElapsedTicks--;
            }
            if (currentGame != null &&
                currentGame.NetworkHandler != null &&
                currentGame.NetworkHandler.IsHost
                && !currentGame.NetworkHandler.OfflineMode
                && currentGame.CurrentTick % 100 == 0)
            {
                var dataPackage = new StaticEngine.NetworkingPackage_ServerDataObj();
                dataPackage.gameTick = currentGame.CurrentTick;
                dataPackage.currOsMs = Environment.TickCount;
                dataPackage.mousePointer = (StaticEngine.MouseX, StaticEngine.MouseY);
                dataPackage.tickAcceleration = StaticEngine.TickAcceleration;

                Console.WriteLine("-§--§--§---------");
                Console.WriteLine("Sending Data: gameTick: "+currentGame.CurrentTick.ToString());
                Console.WriteLine("Ticks every send"+ x.ToString());
                Console.WriteLine("-§--§--§---------");
                
                currentGame.NetworkHandler.InvokeEvent("ReceiveServerTick", dataPackage);
            }

            { // Framerate aktualisieren (falls geändert)
                int fps = Math.Max(1, 1000 / StaticEngine.Framerate);
                if (tmrGameUpdate.Interval != fps)
                    tmrGameUpdate.Interval = fps;
            }

            Refresh();
        }
        private ulong[] serverTicks = new ulong[5] {0l,0l,0l,0l,0l};
        private int[] serverTickTimes = new int[5] { 0, 0, 0, 0, 0 };
        private int selector = 0;
        public void ReceiveServerTick(object s)
        {
            Console.WriteLine("| Received event");
            if (currentGame == null ||
                currentGame.NetworkHandler == null ||
                currentGame.NetworkHandler.IsHost) return;
            var dataPackage = s as StaticEngine.NetworkingPackage_ServerDataObj;
            Console.WriteLine("| Received some ServerData: gametick: " + dataPackage.gameTick.ToString());

            float avgTickChange, avgTimeFrame;
            serverTicks[selector%serverTicks.Length] = dataPackage.gameTick;
            serverTickTimes[selector++ % serverTickTimes.Length] = dataPackage.currOsMs;
            if (selector >= serverTicks.Length)
            {
                // calculates the average difference between ticks   (with a bias. meaning that the most recent values should have a higher influence)
                float biasExp = 2f; //  = 0 means no bias
                float biasSum = 0; int sumTickChange = 0, sumTimeFrame = 0;
                int forIEnd = serverTicks.Length;
                for (int i = 1; i < forIEnd; i++)
                {
                    float bias = (float)Math.Pow((forIEnd - i) / (float)forIEnd, biasExp);
                    biasSum += bias-1;
                    sumTickChange += (int)(
                        (
                            // calculate tick change in a timeframe and then apply bias
                            (serverTicks[(selector-i)%serverTicks.Length] - serverTicks[(selector - i - 1) % serverTicks.Length])
                            /
                            (double)(serverTickTimes[(selector - i) % serverTicks.Length] - serverTickTimes[(selector - i - 1) % serverTicks.Length])
                        ) * bias); // multiplied with bias
                    sumTimeFrame += (int)(
                        (
                            // calculate timeframe and then apply bias
                            serverTickTimes[(selector - i) % serverTicks.Length] - serverTickTimes[(selector - i - 1) % serverTicks.Length]
                        ) * bias); // multiplied with bias
                }
                avgTickChange = sumTickChange / (serverTicks.Length + biasSum);
                avgTimeFrame = sumTimeFrame / (serverTicks.Length + biasSum);

                // TODO noch zu machen
                //  - schauen ob die ticks aus dem letzten angekommenen package
                // bereits von der engine erreicht sind? wenn nicht ddann engine nachholen
                //  - die avgTickChange und avgTime verwenden um einzuplanen,
                // dass die Engine bei t=x y-viele ticks
                // haben muss (durch if(x > ... && ticks < y) oder so)
                // und wenn nicht dann soll sie direkt die prognostiziert fehlenden Ticks nachholen.


            }


        }
        #endregion

        
    }
}
