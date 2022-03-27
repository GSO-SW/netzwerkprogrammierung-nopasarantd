using NoPasaranTD.Networking;
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
        internal static ulong ElapsedHostTicks { get; set; }
        internal static float HostAvgTickChange { get; set; }
        internal static float HostAvgTimeFrame { get; set; }


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
            /*if (referenceValueOSticks < Environment.TickCount + 1000)
            {
                referenceValueOSticks = Environment.TickCount;
                
            }*/
            int currTick = Environment.TickCount;
            int deltaTick = currTick - lastTick;
            ElapsedTicks += ((ulong)deltaTick)*TickAcceleration;
            lastTick = currTick;
        }
        [Serializable]
        public class NetworkingPackage_ServerData
        {
            public uint gameTick = 0;
            public ulong tickAcceleration = 0;
            public int currOsMs = 0;
        }
        #endregion

    }

}
