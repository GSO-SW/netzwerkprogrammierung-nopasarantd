using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NoPasaranTD.Model
{
    [Serializable]
    public abstract class Tower
    {
        public TowerTargetMode TargetMode { get; set; } = TowerTargetMode.Farthest;
        public Rectangle Hitbox { get; set; } // TODO should size of rectangle be accessable?
        public uint Level { get; set; } = 1;

        public ulong NumberKills { get; set; }
        public bool IsSelected { get; set; } = true;
        public bool IsPositionValid { get; set; }
        public bool IsPlaced { get; set; }

        public List<int> SegmentsInRange { get; private set; }
        public List<Vector2D> NotVisibleSpots { get; private set; }

        public uint Strength => StaticInfo.GetTowerDamage(GetType()) * Level;
        public uint Delay => StaticInfo.GetTowerDelay(GetType()) / (Level * 2);
        public double Range => StaticInfo.GetTowerRange(GetType()) * Level * 0.3;
        public uint UpgradePrice => StaticInfo.GetTowerUpgradePrice(GetType()) * Level;
        public uint SellPrice => (uint)(StaticInfo.GetTowerPrice(GetType()) * 0.5) + StaticInfo.GetTowerUpgradePrice(GetType()) * (Level - 1);
        public bool LevelCap => CheckLevelCap();
        public Guid ID { get; } = Guid.NewGuid();

        public Func<Balloon, Balloon, bool> GetBalloonFunc
        {
            get
            {
                switch (TargetMode)
                {
                    case TowerTargetMode.Farthest: return FarthestBallonCheck;
                    case TowerTargetMode.FarthestBack: return FarthestBackBallonCheck;
                    case TowerTargetMode.Strongest: return StrongestBallonCheck;
                    case TowerTargetMode.Weakest: return WeakestBallonCheck;
                    default: throw new Exception("TowerTargetMode not found");
                }
            }
        }

        /// <summary>
        /// Kontrolliert ob der Tower unter dem LevelCap ist
        /// </summary>
        /// <returns>Gibt true zurück, wenn das Level um 1 inkrementiert werden kann, ohne den LevelCap zu überschreiten</returns>
        public bool CheckLevelCap()
        {
            if (StaticInfo.GetTowerLevelCap(GetType()) > Level)
            {
                return true;
            }

            return false;
        }

        public abstract void Render(Graphics g);
        public abstract void Update(Game game);

        #region BalloonChecks
        /// <summary>
        /// Kontrolliert, ob der Ballon Check weiter auf dem Pfad ist als der Ballon bCurrent
        /// </summary>
        /// <param name="bCheck">Zu kontrollierender Ballon</param>
        /// <param name="bCurrent">Derzeitiger Ballon</param>
        /// <returns>True wenn der Ballon Check weiter ist als der Ballon bCurrent</returns>
        public bool FarthestBallonCheck(Balloon bCheck, Balloon bCurrent)
        {
            return bCheck.PathPosition > bCurrent.PathPosition;
        }

        public bool FarthestBackBallonCheck(Balloon bCheck, Balloon bCurrent)
        {
            return bCheck.PathPosition < bCurrent.PathPosition;
        }

        public bool StrongestBallonCheck(Balloon bCheck, Balloon bCurrent)
        {
            return bCheck.Strength > bCurrent.Strength;
        }

        public bool WeakestBallonCheck(Balloon bCheck, Balloon bCurrent)
        {
            return bCheck.Strength < bCurrent.Strength;
        }
        #endregion

        /// <summary>
        /// Bestimmt alle Pfadeigenschaften für den Turm abhängig, von der Reichweite, also welche Segmente in Reichweite sind und welche Teile durch ein Hinderniss verdeckt sind
        /// </summary>
        /// <param name="map"></param>
        public void SearchSegments(Map map)
        {
            Rectangle rectangle = map.GetScaledRect(StaticEngine.RenderWidth, StaticEngine.RenderHeight, Hitbox); // Das skallierte Rechteck des Turmes
            Vector2D centreP = new Vector2D(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2); // Das Zentrum des Turmes welches als Anhaltspunkt für die Reichweite genommen wird
            FindSegmentsInRange(map, centreP);
            FindObstaclesInTheWay(map, centreP);
        }

        /// <summary>
        /// Bestimmt alle Pfadsegmente die in der Reichweite des Turmes sind
        /// </summary>
        /// <param name="map"></param>
        private void FindSegmentsInRange(Map map, Vector2D centreP)
        {
            List<int> segments = new List<int>();
            for (int i = 0; i < map.BalloonPath.Length - 1; i++)
            {
                Vector2D locationP = new Vector2D(map.BalloonPath[i].X, map.BalloonPath[i].Y);
                Vector2D directionP = new Vector2D(map.BalloonPath[i + 1].X - locationP.X, map.BalloonPath[i + 1].Y - locationP.Y);
                float factor = -1 * (directionP.X * (locationP.X - centreP.X) + directionP.Y * (locationP.Y - centreP.Y)) / (directionP.X * directionP.X + directionP.Y * directionP.Y);
                if (factor > 1)
                {
                    factor = 1;
                }
                else if (factor < 0)
                {
                    factor = 0;
                }

                Vector2D closestP = locationP + factor * directionP;
                if ((closestP - centreP).Magnitude <= Range)
                {
                    segments.Add(i);
                }
            }
            SegmentsInRange = segments;
        }

        private void FindObstaclesInTheWay(Map map, Vector2D centreP)
        {
            List<Vector2D> blindSpots = new List<Vector2D>();
            for (int j = 0; j < SegmentsInRange.Count; j++)// Alle Pfadstücke des Ballonpfades durchgehen
            {
                foreach (Obstacle item in map.Obstacles) // Alle Hindernisse durchgehen
                {
                    Vector2D[] cornersV = map.GetRectangleCorners(item.Hitbox); // Alle Eckpunkte des Hindernisses bestimmen
                    List<float> pathValues = new List<float>();
                    for (int i = 0; i < cornersV.Length; i++) // Alle Eckpunkte durchgehen
                    {
                        Vector2D connectionRecV = cornersV[i] - centreP; // Richtungsvektor zwischen Turm und der Ecke
                        if ((centreP + connectionRecV).MagnitudeSquared < Range * Range) // Länge des Vektors quadrieren, um die Wurzeloperation zu sparen
                        {
                            Vector2D locationPathV = map.BalloonPath[SegmentsInRange[j]]; // Ortsvektor der Gerade für den Pfad
                            Vector2D connectionPathV = map.BalloonPath[SegmentsInRange[j] + 1] - locationPathV; // Richtungsvektor der Gerade für den Pfad
                            float factorPath = Vector2D.Intersection(locationPathV, connectionPathV, centreP, connectionRecV);
                            float factorRec = Vector2D.Intersection(centreP, connectionRecV, locationPathV, connectionPathV);
                            if (factorPath > 0 && factorPath < 1 && factorRec > 1 && (connectionRecV * factorRec).MagnitudeSquared < Range * Range) // Der Schnittpunkt ist innerhalb des Intervals des Pfades und hinter dem Hindernis
                            {
                                pathValues.Add((float)(map.GetFragmentMagnitudeTo(SegmentsInRange[j] - 1) + (connectionPathV * factorPath).Magnitude));
                            }
                        }
                    }

                    if (pathValues.Count == 0)
                    {
                        if (CheckPathPointBlock(map.BalloonPath[SegmentsInRange[j]], centreP, cornersV) || CheckPathPointBlock(map.BalloonPath[SegmentsInRange[j] + 1], centreP, cornersV))
                        {
                            blindSpots.Add(new Vector2D(map.GetFragmentMagnitudeTo(SegmentsInRange[j] - 1), map.GetFragmentMagnitudeTo(SegmentsInRange[j]))); // j muss um 1 nach hinten verschoben werden, da immer die länge bis zum nächsten Stück berechnet wird
                        }
                    }
                    else if (pathValues.Count == 1) // Nur ein Schnittpunkt
                    {   // Einer der Eckpunkte muss der nächste Endpunkt sein für dieses Pfadstück
                        if (CheckPathPointBlock(map.BalloonPath[SegmentsInRange[j]], centreP, cornersV)) // Der Eckpunkt davor ist im Schatten des Hindernisses
                        {
                            blindSpots.Add(new Vector2D(map.GetFragmentMagnitudeTo(SegmentsInRange[j] - 1), pathValues[0]));
                        }
                        else // Der Eckpunkt danach ist im Schatten des Hindernisses
                        {
                            blindSpots.Add(new Vector2D(pathValues[0], map.GetFragmentMagnitudeTo(SegmentsInRange[j] - 1)));
                        }
                    }
                    else // Mehr als 1 Schnittpunkt wurde gefunden
                    {
                        float lowerBound = pathValues[0];
                        float upperBound = pathValues[0];
                        foreach (float pValue in pathValues)
                        {
                            if (lowerBound > pValue) // Sortieren der Werte
                            {
                                lowerBound = pValue;
                            }
                            else if (upperBound < pValue)
                            {
                                upperBound = pValue;
                            }
                        }
                        if (CheckPathPointBlock(map.BalloonPath[SegmentsInRange[j]], centreP, cornersV)) // Kontrollieren, dass die Enden des Pfades nicht verdeckt sind
                        {
                            lowerBound = (float)map.GetFragmentMagnitudeTo(SegmentsInRange[j] - 1);
                        }

                        if (CheckPathPointBlock(map.BalloonPath[SegmentsInRange[j] + 1], centreP, cornersV))
                        {
                            upperBound = (float)map.GetFragmentMagnitudeTo(SegmentsInRange[j]);
                        }

                        blindSpots.Add(new Vector2D(lowerBound, upperBound));
                    }
                }
            }
            NotVisibleSpots = blindSpots;
        }

        /// <summary>
        /// Kontrolliert, ob ein "Rechteck"
        /// </summary>
        /// <param name="pathLocV">Punkt der kontrolliert werden soll</param>
        /// <param name="towerCentre">Mittelpunkt der Turmhitbox</param>
        /// <param name="cornersV">Ecken des Hindernisses</param>
        /// <returns>Gibt true zurück wenn der Pfadpunkt im Schatten des Rechtecks liegt</returns>
        private bool CheckPathPointBlock(Vector2D pathLocV, Vector2D towerCentre, Vector2D[] cornersV)
        {
            Vector2D towerPathV = pathLocV - towerCentre;
            for (int i = 0; i < cornersV.Length; i++)
            {
                Vector2D connectionRecV;
                if (i != cornersV.Length - 1) // Die connection ist immer mit dem nächsten Punkt in der Reihe
                {
                    connectionRecV = cornersV[i + 1] - cornersV[i];
                }
                else // Bei dem letzten punkt wieder auf den ersten springen
                {
                    connectionRecV = cornersV[0] - cornersV[i];
                }

                float factorPathTower = Vector2D.Intersection(pathLocV, towerPathV, cornersV[i], connectionRecV);
                float factorObstacle = Vector2D.Intersection(cornersV[i], connectionRecV, pathLocV, towerPathV);
                if (factorPathTower <= 1 && factorPathTower >= 0 && factorObstacle <= 1 && factorObstacle >= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }

    [Serializable]
    public enum TowerTargetMode
    {
        Farthest,
        FarthestBack,
        Strongest,
        Weakest,
    }
}
