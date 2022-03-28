using System;

namespace NoPasaranTD.Engine
{
    public static class StaticEngine
    {

        #region Bildschirmeigenschaften
        public static int RenderWidth { get; set; } = 1280;
        public static int RenderHeight { get; set; } = 720;
        public static int Framerate { get; set; } = 120;

        public static int MouseX { get; internal set; }
        public static int MouseY { get; internal set; }
        #endregion

        #region Engine region
        internal static ulong ElapsedTicks { get; set; }

        /// <summary>
        /// Ein Wert um welchen die Tickinkrementierung beschleunigt werden soll
        /// </summary>
        internal static ulong TickAcceleration { get; set; } = 1;

        // TODO: Ändern zu jetzige Server-Ticks
        private static int lastTick = Environment.TickCount;

        /// <summary>
        /// Errechnet die vergangenen Ticks seit dem letzten Aufruf dieser Methode
        /// </summary>
        internal static void Update()
        {
            // TODO: Ändern zu jetzige Server-Ticks
            int currTick = Environment.TickCount;
            int deltaTick = currTick - lastTick;
            ElapsedTicks += ((ulong)deltaTick) * TickAcceleration;
            lastTick = currTick;
        }
        #endregion

    }

}
