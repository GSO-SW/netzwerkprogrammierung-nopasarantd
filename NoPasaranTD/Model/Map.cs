using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NoPasaranTD.Model
{
    struct Fragment
    {
        public Fragment(int sv, double sp, double ep)
            => (StartVector, StartPercent, EndPercent) = (sv, sp, ep);

        public int StartVector { get; }
        public double StartPercent { get; }
        public double EndPercent { get; }
    }

    public class Map
    {
        public List<Obstacle> Obstacles { get; set; }        
        public Vector2D[] BalloonPath { get; set; }

        public string BackgroundPath { get; private set; }
        public Bitmap BackgroundImage { get; private set; }

        // Einzelne Fragmente (Errechnet im setter von 'BalloonPath')
        private Fragment[] pathFragments;

        /// <summary>
        /// Inizialisiert das Objekt und berechnet somit alle Pfadfragmente.<br/>
        /// Erst nachdem diese Methode (nach jeder Änderung von 'BalloonPath') ausgeführt wird, gibt 'GetPathPosition' ein richtiges Ergebnis.
        /// </summary>
        public void Initialize()
        {
            pathFragments = new Fragment[BalloonPath.Length - 1];

            // Benutzung von Methoden aus Klasse, da 'Path' schon gesetzt ist
            double totalMagnitude = GetFragmentMagnitudeTotal();

            double startMagnitude = 0;
            for (int i = 0; i < pathFragments.Length; i++)
            {
                double endSum = GetFragmentMagnitudeTo(i);
                // StartVector, StartPercent, EndPercent
                pathFragments[i] = new Fragment(i, startMagnitude / totalMagnitude, endSum / totalMagnitude);
                startMagnitude = endSum;
            }
        }

        /// <summary>
        /// Errechnet die Position auf dem Pfad anhand von Prozent.<br/>
        /// Achtung: Bei änderung von 'BalloonPath' stelle sicher, dass die 'Initialize'-Methode ausgeführt wurde!
        /// </summary>
        /// <param name="percent">Position auf dem Pfad in Prozent</param>
        /// <returns>Errechnete Position relational nach Prozent</returns>
        public Vector2D GetPathPosition(double percent)
        {
            Fragment fragment = pathFragments[
                (int)((pathFragments.Length - 1) * percent)
            ];

            Vector2D start = BalloonPath[fragment.StartVector];
            Vector2D end = BalloonPath[fragment.StartVector + 1];

            return start + (end - start) // Geradengleichung aufstellen,
                / (fragment.EndPercent - fragment.StartPercent) // normalisieren
                * (percent - fragment.StartPercent); // & mithilfe von differenz zwischen 'StartPercent' & 'percent' faktorisieren
        }

        // Berechnen des Betrags vom Pfad
        private double GetFragmentMagnitudeTotal()
            => GetFragmentMagnitudeTo(BalloonPath.Length - 2);

        // Berechnen des Betrags zwischen Punkt 'index' und 'index + 1'
        private double GetFragmentMagnitudeOf(int index)
            => (BalloonPath[index + 1] - BalloonPath[index]).Magnitude;

        // Berechnen des Betrags zwischen Punkt '(0, 0)' und 'index'
        private double GetFragmentMagnitudeTo(int index)
        {
            double sum = 0;
            for (int i = 0; i <= index; i++)
                sum += GetFragmentMagnitudeOf(i);
            return sum;
        }

    }
}
