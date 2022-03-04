using System;
using System.Diagnostics;
using System.Threading;

namespace NoPasaranTD.Engine
{
    /// <summary>
    /// Klasse zum errechnen von Durchschnitten
    /// </summary>
    internal class RunningAverage
    {
        private const int DAMPEN_THRESHOLD = 10;
        private const float DAMPEN_FACTOR = 0.9f;

        private int offset;
        private readonly long[] slots;
        public RunningAverage(int slots, long value)
        {
            this.slots = new long[slots];
            for (offset = 0; offset < slots; offset++)
                this.slots[offset] = value;
        }

        /// <summary>
        /// Fügt ein Element für die Errechnung vom Durchschnitt hinzu.<br/>
        /// Falls das Array voll ist, werden die ersten Elemente überschrieben.
        /// </summary>
        public void Add(long value)
        {
            slots[offset++ % slots.Length] = value;
            offset %= slots.Length;
        }

        /// <summary>
        /// Errechnet den Durchschnitt aller vorhandenen Elemente.
        /// </summary>
        /// <returns>Durchschnitt von den gespeicherten Elementen</returns>
        public long GetAverage()
        {
            long sum = 0;
            for(long i = 0; i < slots.Length; i++)
                sum += slots[i];
            return sum / slots.Length;
        }

        /// <summary>
        /// Dämpft den Durchschnitt falls ein bestimmter überschritten wurde.
        /// </summary>
        public void Damp()
        {
            if(GetAverage() > DAMPEN_THRESHOLD)
            {
                for (int i = 0; i < slots.Length; i++)
                    slots[i] = (long)(slots[i] * DAMPEN_FACTOR);
            }
        }
    }

    public static class Engine
    {

        #region Bildschirm Eigenschaften
        public static int RenderWidth { get; set; } = 1280;
        public static int RenderHeight { get; set; } = 720;
        public static int Framerate { get; set; } = 120;

        public static int MouseX { get; internal set; }
        public static int MouseY { get; internal set; }
        #endregion
        
        #region Synchronize region
        private static readonly Stopwatch sleepWatch = new Stopwatch();
        private static readonly RunningAverage sleepAverage = new RunningAverage(10, 1);
        private static long nextFrame = Environment.TickCount;

        /// <summary>
        /// Präzises warten zwischen einzelnen Frames anhand von eingestellter Framerate.
        /// </summary>
        internal static void Sync()
        {
            while (nextFrame - Environment.TickCount > sleepAverage.GetAverage())
            {
                sleepWatch.Restart();
                Thread.Sleep(1);
                sleepWatch.Stop();

                sleepAverage.Add(sleepWatch.ElapsedMilliseconds);
            }
            sleepAverage.Damp();

            nextFrame = Math.Max(nextFrame + 1000 / Framerate, Environment.TickCount);
        }
        #endregion

    }

}
