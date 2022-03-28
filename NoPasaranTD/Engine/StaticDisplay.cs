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
                LoadScreen(null); // Screen entladen
                // Lade map und initialisiere sie
                Map map = MapData.GetMapByFileName(mapFile); map.Initialize();
                currentGame = new Game(map, handler, this);
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

        int environTime = 0, tickswork = 0, lastOnlineUpdate = 0, periodStart = 0, periodTicks = 0;
        private void tmrGameUpdate_Tick(object sender, EventArgs e)
        {
            StaticEngine.Update();
            bool IsNetworkReady = currentGame != null
                && currentGame.NetworkHandler != null;
            bool IamHost = IsNetworkReady
                && currentGame.NetworkHandler.IsHost
                && !currentGame.NetworkHandler.OfflineMode;
            if (IamHost) StaticEngine.Update();
            if (IsNetworkReady && currentGame.NetworkHandler.LastUpdate+100 < Environment.TickCount) currentGame.NetworkHandler.Update();
            while (IamHost && StaticEngine.ElapsedTicks > 0)
            {
                currentGame?.Update();
                currentScreen?.Update();
                StaticEngine.ElapsedTicks--;
            }

            float someVal = TweenService(periodStart, periodStart + StaticEngine.HostAvgTimeFrame, 0, StaticEngine.HostAvgTickChange, Environment.TickCount);
            if (Environment.TickCount > periodStart + StaticEngine.HostAvgTimeFrame)
            {
                periodTicks = 0;
                periodStart = Environment.TickCount;
            }
            while (!IamHost && IsNetworkReady
                && (StaticEngine.ElapsedHostTicks > 0 ||
                    (StaticEngine.ElapsedHostTicks == 0 &&
                        periodTicks < someVal
                    )
                   )
                  )
            {
                periodTicks++;

                if (Environment.TickCount >= environTime)
                {
                    Console.WriteLine("Ticks this second: "+tickswork.ToString());
                    environTime = Environment.TickCount + 1000;
                    tickswork = 0;
                }
                tickswork++;

                lastOnlineUpdate = Environment.TickCount;
                if (StaticEngine.ElapsedHostTicks > 0) StaticEngine.ElapsedHostTicks--;
                //StaticEngine.ElapsedHostTicks -= (StaticEngine.ElapsedHostTicks == 0 ? 0 : 1);
                currentGame?.Update();
                currentScreen?.Update();
                if (currentGame.CurrentTick % 100 == 0) Console.WriteLine(Environment.TickCount.ToString() + "ms os time  |  current exec ticks left: "+StaticEngine.ElapsedHostTicks.ToString());
            }


            // if isHost dann jede X Ticks: sende Current Gametick an andere Teilnehmer
            if (IamHost && currentGame.CurrentTick % 400 == 0)
            {
                var dataPackage = new StaticEngine.NetworkingPackage_ServerData();
                dataPackage.gameTick = currentGame.CurrentTick;
                dataPackage.currOsMs = Environment.TickCount;
                dataPackage.tickAcceleration = StaticEngine.TickAcceleration;

                Console.WriteLine("SERVER: Sending Data: gameTick: "+currentGame.CurrentTick.ToString());
                
                currentGame.NetworkHandler.InvokeEvent("ReceiveServerTick", dataPackage, true);
            }

            { // Framerate aktualisieren (falls geändert)
                int fps = Math.Max(1, 1000 / StaticEngine.Framerate);
                if (tmrGameUpdate.Interval != fps)
                    tmrGameUpdate.Interval = fps;
            }

            Refresh();
        }
        private static float TweenService(float tickStart, float tickEnd, float minValue, float maxValue, int currentTick)
        {
            float deltaValue = maxValue - minValue, deltaTick = tickEnd - tickStart;
            float factor = Math.Min(Math.Max(currentTick - tickStart, 0), deltaTick) / deltaTick;  //  E[0;1]
            return minValue + deltaValue * factor;
        }
        private ulong[] serverTicks = new ulong[5] {0L,0L,0L,0L,0L};
        private int[] serverTickTimes = new int[5] { 0, 0, 0, 0, 0 };
        private int selector = 0;
        public void ReceiveServerTick(object s)
        {
            if (currentGame == null ||
                currentGame.NetworkHandler == null ||
                currentGame.NetworkHandler.IsHost) return;
            var dataPackage = s as StaticEngine.NetworkingPackage_ServerData;
            Console.WriteLine("| Received some ServerData: gametick: " + dataPackage.gameTick.ToString());

            double avgTickChange, avgTimeFrame;
            serverTicks[selector%serverTicks.Length] = dataPackage.gameTick;
            serverTickTimes[selector++ % serverTickTimes.Length] = dataPackage.currOsMs;
            if (selector >= serverTicks.Length)
            {
                // calculates the average difference between ticks   (with a bias. meaning that the most recent values should have a higher influence)
                float biasExp = 2f; //  = 0 means no bias
                float biasSum = 0; double sumTickChange = 0, sumTimeFrame = 0;
                int forloopIEnd = serverTicks.Length;
                for (int i = 1; i < forloopIEnd; i++)
                {
                    float bias = (float)Math.Pow((forloopIEnd - i) / (float)forloopIEnd, biasExp);
                    biasSum += bias-1;
                    sumTickChange += 
                        (
                            // calculate tick change in a timeframe and then apply bias
                            (serverTicks[(selector-i)%serverTicks.Length] - serverTicks[(selector - i - 1) % serverTicks.Length])
                            
                        ) * bias; // multiplied with bias
                    sumTimeFrame += (int)(
                        (
                            // calculate timeframe and then apply bias
                            serverTickTimes[(selector - i) % serverTicks.Length] - serverTickTimes[(selector - i - 1) % serverTicks.Length]
                        ) * bias); // multiplied with bias
                }
                avgTickChange = sumTickChange / (serverTicks.Length + biasSum);
                avgTimeFrame = sumTimeFrame / (serverTicks.Length + biasSum);

                if (avgTickChange < 0)
                    ;

                StaticEngine.HostAvgTickChange = (int)avgTickChange;
                StaticEngine.HostAvgTimeFrame = (int)avgTimeFrame;

                // TODO noch zu machen
                //  - schauen ob die ticks aus dem letzten angekommenen package
                // bereits von der engine erreicht sind? wenn nicht ddann engine nachholen
                //  - die avgTickChange und avgTime verwenden um einzuplanen,
                // dass die Engine bei t=x y-viele ticks
                // haben muss (durch if(x > ... && ticks < y) oder so)
                // und wenn nicht dann soll sie direkt die prognostiziert fehlenden Ticks nachholen.


                // berechnet und legt fest die minimale Schranke die erreicht sein sollte die AUF JEDEN FALL zum nachholen sind
                //  [ es sind aber dann noch die Ticks die während der Lieferung des Packets vergangen sind nachzuholen,
                //  weil die Daten immer veraltet sind muss prognostiziert werden ]
                ulong deltaHostClientTick = 0;
                if (dataPackage.gameTick > currentGame.CurrentTick);
                    deltaHostClientTick = dataPackage.gameTick - currentGame.CurrentTick;
                StaticEngine.ElapsedHostTicks = deltaHostClientTick; 





            }


        }
        #endregion

        
    }
}
