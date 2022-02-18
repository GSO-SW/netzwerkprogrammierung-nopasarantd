using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace NoPasaranTD.Model
{
    struct Fragment
    {
        public Fragment(int sv, double sl, double el)
            => (StartVector, StartLength, EndLength) = (sv, sl, el);

        public int StartVector { get; }
        public double StartLength { get; }
        public double EndLength { get; }
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class Map
    {
        public List<Obstacle> Obstacles { get; set; }
        public Vector2D[] BalloonPath { get; set; }

        [JsonIgnore]
        public Bitmap BackgroundImage { get; private set; }

        private string backgroundPath;
        public string BackgroundPath 
        { 
            get => backgroundPath; 
            set
            { 
                backgroundPath = value;
                BackgroundImage = new Bitmap(Environment.CurrentDirectory + BackgroundPath); 
            } 
        }

        [JsonIgnore]
        public double PathLength { get; private set; }

        // Einzelne Fragmente (Errechnet im setter von 'BalloonPath')
        private Fragment[] pathFragments;

        /// <summary>
        /// Inizialisiert das Objekt und berechnet somit die Länge des Pfades und dessen einzelne Fragmente.<br/>
        /// Erst nachdem diese Methode (nach jeder Änderung von 'BalloonPath') ausgeführt wird, gibt 'GetPathPosition' & 'PathLength' ein richtiges Ergebnis.
        /// </summary>
        public void Initialize()
        {
            // Pfadlänge berechnen
            PathLength = GetFragmentMagnitudeTo(BalloonPath.Length - 2);
        
            double startMagnitude = 0;
            pathFragments = new Fragment[BalloonPath.Length - 1];
            for (int i = 0; i < pathFragments.Length; i++)
            { // Errechnen der einzelnen Fragmente
                double endSum = GetFragmentMagnitudeTo(i);
                // StartVector, StartPercent, EndPercent
                pathFragments[i] = new Fragment(i, startMagnitude, endSum);
                startMagnitude = endSum;
            }
        }

        /// <summary>
        /// Errechnet die Position auf dem Pfad anhand von der gegebenen Länge.<br/>
        /// Achtung: Bei änderung von 'BalloonPath' stellen Sie sicher, dass die 'Initialize'-Methode ausgeführt wurde!
        /// </summary>
        /// <param name="length">Position auf dem Pfad in Längeneinheit</param>
        /// <returns>Errechnete Position relational nach der Länge</returns>
        public Vector2D GetPathPosition(double length)
        {
            // Auswahl eines ungefähr richtigen Fragments
            int index = (int)((pathFragments.Length - 1) / PathLength * length);
            Fragment fragment = pathFragments[index];

            // Suche nach dem genau richtigen Fragment
            while (fragment.StartLength > length) 
                fragment = pathFragments[--index];
            while (fragment.EndLength < length) 
                fragment = pathFragments[++index];

            Vector2D start = BalloonPath[fragment.StartVector];
            Vector2D end = BalloonPath[fragment.StartVector + 1];

            return start + (end - start) // Geradengleichung aufstellen,
                / (fragment.EndLength - fragment.StartLength) // normalisieren
                * (length - fragment.StartLength); // & mithilfe von differenz zwischen 'StartLength' & 'length' faktorisieren
        }
        
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
