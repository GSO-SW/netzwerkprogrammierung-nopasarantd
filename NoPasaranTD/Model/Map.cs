using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using NoPasaranTD.Data;
using System.Reflection;
using System.IO;

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
        public string Name { get; set; }
        public Size Dimension { get; set; }
        public List<Obstacle> Obstacles { get; set; }
        public Vector2D[] BalloonPath { get; set; }
        public float PathWidth { get; set; }

        private string backgroundPath;
        public string BackgroundPath
        {
            get => backgroundPath;
            set
            {
                backgroundPath = value;

                Assembly assembly = Assembly.GetExecutingAssembly();
                string resourceName = "NoPasaranTD.Resources." + value;
                using(Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    BackgroundImage = new Bitmap(stream);
                }
            }
        }

        [JsonIgnore]
        public Bitmap BackgroundImage { get; private set; }

        [JsonIgnore]
        public double PathLength { get; private set; }

        // Mehrdimensional um die Hitbox unter und oberhalb des Pfades zu speichern
        private Vector2D[,] pathHitbox;

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

            pathHitbox = GetPathHitbox();
        }

        /// <summary>
        /// Errechnet die Position auf dem Pfad anhand von der gegebenen Länge und übersetzt diese auf die Bildschirmposition.<br/>
        /// Achtung: Bei änderung von 'BalloonPath' stellen Sie sicher, dass die 'Initialize'-Methode ausgeführt wurde!
        /// </summary>
        /// <param name="scaledWidth">Breite des Renderbereiches</param>
        /// <param name="scaledHeight">Höhe des Renderbereiches</param>
        /// <param name="length">Position auf dem Pfad in Längeneinheit</param>
        /// <returns>Errechnete Position relational nach der Länge</returns>
        public Vector2D GetPathPosition(int scaledWidth, int scaledHeight, double length)
        {
            Vector2D originalPos = GetPathPosition(length);
            return new Vector2D(
                originalPos.X / Dimension.Width * scaledWidth,
                originalPos.Y / Dimension.Height * scaledHeight
            );
        }

        public bool IsCollidingWithPath(int scaledWidth, int scaledHeight, Rectangle rect)
        {
            return IsCollidingWithPath(new Rectangle(
                (int)((float)rect.X / scaledWidth * Dimension.Width),
                (int)((float)rect.Y / scaledHeight * Dimension.Height),
                (int)((float)rect.Width / scaledWidth * Dimension.Width),
                (int)((float)rect.Height / scaledHeight * Dimension.Height)
            ));
        }

        /// <summary>
        /// Errechnet die Position auf dem Pfad anhand von der gegebenen Länge.<br/>
        /// Achtung: Bei änderung von 'BalloonPath' stellen Sie sicher, dass die 'Initialize'-Methode ausgeführt wurde!
        /// </summary>
        /// <param name="length">Position auf dem Pfad in Längeneinheit</param>
        /// <returns>Errechnete Position relational nach der Länge</returns>
        private Vector2D GetPathPosition(double length)
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

        /// <summary>
        /// Kontrolliert, ob das angegebene Recheck einen Abstand von X Einheiten zum nächsten Pfadpunkt hat und ob keine Hitbox des Pfades getroffen wurde
        /// </summary>
        /// <param name="rect">Zu überprüfendes Rechteck</param>
        /// <returns>False wenn es eine Überschneidung gibt</returns>
        private bool IsCollidingWithPath(Rectangle rect)
        {
            Vector2D[] cornersV = new Vector2D[4]; // Speichern der Ecken des Rechtecks
            for (int i = 0; i < 2; i++) // Alle Ecken durchgehen
                for (int j = 0; j < 2; j++)
                    cornersV[i * 2 + j] = new Vector2D(rect.X + i * rect.Width, rect.Y + j * rect.Height); // Ecken abspeichern
            Vector2D save = cornersV[2]; // Ecke 2 mit 3 tauschen um eine durchgehende Reihenfolge zu haben
            cornersV[2] = cornersV[3];
            cornersV[3] = save;

            for (int i = 0; i < cornersV.Length; i++) // Alle Ecken durchgehen
            {
                Vector2D connectionRecV;
                if (i != cornersV.Length - 1) // Die connection ist immer mit dem nächsten Punkt in der Reihe
                    connectionRecV = cornersV[i + 1] - cornersV[i];
                else // Bei dem letzten punkt wieder auf den ersten springen
                    connectionRecV = cornersV[0] - cornersV[i];

                //Durchgehen aller Pfadpunkte und schauen ob innerhalb des Radius ein Stück des Rechtecks ist
                foreach (var item in BalloonPath)
                {
                    // Nächster Punkt auf der Gerade des Rechtecks berechnet
                    float closestPointDistance = -1 * (((cornersV[i].X - item.X) * connectionRecV.X + (cornersV[i].Y - item.Y) * connectionRecV.Y) / (connectionRecV.X * connectionRecV.X + connectionRecV.Y * connectionRecV.Y));
                    if (closestPointDistance < 0) // Sollte der Punkt außerhalb der Länge des Rechtecks liegen wird auf die entsprechende Seite gesetzt
                        closestPointDistance = 0;
                    else if (closestPointDistance > 1)
                        closestPointDistance = 1;

                    if ((cornersV[i] + closestPointDistance * connectionRecV - item).Magnitude < PathWidth) // Länge des Verbindungsvektors überprüfen // TODO: Mit StaticInfo verbinden
                        return false;
                }

                // Alle Hitboxen des Pfades durchgehen und auf Kollisionen kontrollieren
                for (int j = 0; j < BalloonPath.Length - 1; j++)
                {
                    for (int k = 0; k < 2; k++) // Hitbox ober- und unterhalb kontrollieren
                    {
                        Vector2D pathLocationV = pathHitbox[k, j * 2];
                        Vector2D pathDirectionV = pathHitbox[k, j * 2 + 1] - pathLocationV;
                        // Wert der Variable für die Geradengleichung an der Schnittstelle
                        float collisionVariablePathF = ((pathLocationV.Y - cornersV[i].Y) * connectionRecV.X + (cornersV[i].X - pathLocationV.X) * connectionRecV.Y) / (pathDirectionV.X * connectionRecV.Y - pathDirectionV.Y * connectionRecV.X);
                        float collisionVariableRecF = ((cornersV[i].Y - pathLocationV.Y) * pathDirectionV.X + (pathLocationV.X - cornersV[i].X) * pathDirectionV.Y) / (connectionRecV.X * pathDirectionV.Y - connectionRecV.Y * pathDirectionV.X);
                        // Kontrolle, ob die Schnittstelle zwischen Gerade und Seite des Rechtecks innerhalb der Intervalle von [0,1] liegt
                        if (collisionVariablePathF >= 0 && collisionVariablePathF <= 1 && collisionVariableRecF >= 0 && collisionVariableRecF <= 1)
                            return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Gibt an ob ein Ballon das nächste Segment des Pfades erreicht hat
        /// </summary>
        /// <returns>Gibt true zurück wenn der nächste Pfadabschnitt erreicht wurde</returns>
        public bool CheckBalloonPosFragment(float pos, uint segmentID)
        {
            if (pos > pathFragments[segmentID].EndLength)
                return true;
            return false;
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

        /// <summary>
        /// Erstellt Parallele Vektoren zur Strecke
        /// </summary>
        /// <returns>Gibt ein zweidimensionales Array zurück mit einer Dimension für überhalb des Pfades und einer unterhalb</returns>
        private Vector2D[,] GetPathHitbox()
        {
            Vector2D[,] pathHitbox = new Vector2D[2, BalloonPath.Length * 2 - 2];
            int directionI = 1;
            for (int i = 0; i < BalloonPath.Length; i++)
            {
                // Erst checken ob es nur einen anliegenden Ballon gibt, also erster / letzter
                if (i != 0)
                    directionI = 0;
                if (i == BalloonPath.Length - 1)
                    directionI = -1;
                for (int j = -1; j < 2; j += 2) // Jeden Punkt 2 mal durchgehen, um für beide verbundenen Geraden parallele zu erstellen
                {
                    if (directionI == 1) // Bei dem ersten nur auf den nächsten Punkt schauen
                        if (j < 0)
                            j = 1;
                    if (directionI == -1) // Bei dem letzten nur auf den Punkt davor schauen
                        if (j > 0)
                            break;

                    // Orthogonaler Richtungsvektor zu beiden nächsten Punkten in beide Richtungen
                    Vector2D directionV = new Vector2D(-1 * (BalloonPath[i + j].Y - BalloonPath[i].Y), BalloonPath[i + j].X - BalloonPath[i].X);
                    // Berechnet die Variable für die Geradengleichung um auf den Punkt zu kommen mit dem ausgewählten Abstand
                    double multiplicatorD = Math.Sqrt((PathWidth * PathWidth) / (directionV.X * directionV.X + directionV.Y * directionV.Y));
                    // Die Berechnete Variable in die Geradengleichung einsetzten um den Punkt zu erhalten
                    Vector2D v1 = new Vector2D(BalloonPath[i].X + directionV.X * multiplicatorD, BalloonPath[i].Y + directionV.Y * multiplicatorD);
                    // In beide Richungen orthogonal vom Vektor aus schauen 
                    Vector2D v2 = new Vector2D(BalloonPath[i].X + directionV.X * multiplicatorD * -1, BalloonPath[i].Y + directionV.Y * multiplicatorD * -1);
                    if (j == 1)
                    {
                        pathHitbox[0, i * 2] = v1;
                        pathHitbox[1, i * 2] = v2;
                    }
                    else
                    {
                        pathHitbox[0, i * 2 - 1] = v2; // Die Punkte müssen "vertauscht" werden, denn die Richtungsvektoren sind gespiegelt jenachdem wie herum sie erstellt werden 
                        pathHitbox[1, i * 2 - 1] = v1;
                    }
                }
            }
            return pathHitbox;
        }
    }
}
