using System;
using System.Diagnostics;

namespace NoPasaranTD.Utilities
{

    public static class QuickDiagnostics
    {
        private static bool isInitialised = false;
        private static uint[] counters;
        private static long[] timeCollectors;
        private static long[] timeCollectorsStart; // cached values
        private static Stopwatch sw;
        private static void Initialise()
        {
            if (isInitialised)
            {
                return;
            }

            isInitialised = true;

            counters = new uint[100];
            timeCollectors = new long[100];

            timeCollectorsStart = new long[timeCollectors.Length];
            for (int i = 0; i < timeCollectorsStart.Length; i++)
            {
                timeCollectorsStart[i] = -1;
            }

            sw = new Stopwatch();
            sw.Start();
        }
        /// <summary>
        /// pop
        /// </summary>
        /// <param name="counterId">up to 100 independent counters possible</param>
        public static void CounterIncrement(int counterId)
        {
            Initialise();
            counters[counterId]++;
        }
        public static void CounterOut(int counterId, bool doReset)
        {
            Initialise();
            Console.WriteLine("<|> COUNTER " + counterId.ToString() + "   has value   " + counters[counterId].ToString());
            if (doReset)
            {
                counters[counterId] = 0;
            }
        }
        public static void TimeCollectorStart(int tCId)
        {
            Initialise();
            if (timeCollectorsStart[tCId] != -1)
            {
                return;
            }

            timeCollectorsStart[tCId] = sw.ElapsedMilliseconds;
        }
        public static void TimeCollectorStop(int tCId)
        {
            Initialise();
            if (timeCollectorsStart[tCId] == -1)
            {
                return;
            }

            timeCollectors[tCId] += sw.ElapsedMilliseconds - timeCollectorsStart[tCId];
            timeCollectorsStart[tCId] = -1;
        }
        public static void TimeCollectorOut(int tCId, bool doReset)
        {
            Initialise();
            Console.WriteLine("<|> TimeCollector " + tCId.ToString() + "   has collected [ms]   " + timeCollectors[tCId].ToString());
            if (doReset) { timeCollectors[tCId] = 0; timeCollectorsStart[tCId] = -1; }
        }
    }
}
