using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NoPasaranTD.Model
{
    public abstract class Tower
    {
        public Rectangle Hitbox { get; set; } // TODO should size of rectangle be accessable?
        public uint Level { get; set; }

        public uint Strength { get => StaticInfo.GetTowerDamage(GetType()); }
        public uint Delay { get => StaticInfo.GetTowerDelay(GetType()); }
        public double Range { get => StaticInfo.GetTowerRange(GetType()); }
        public ulong NumberKills { get; set; }
        public bool IsSelected { get; set; } = true;
        public List<int> SegmentsInRange { get; private set; }


        public Func<Balloon, Balloon, bool> GetBalloonFunc { get; set; }


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
            if (bCheck.PathPosition > bCurrent.PathPosition)
                return true;
            return false;
        }
        public bool FarthestBackBallonCheck(Balloon bCheck, Balloon bCurrent)
        {
            if (bCheck.PathPosition < bCurrent.PathPosition)
                return true;
            return false;
        }
        public bool StrongestBallonCheck(Balloon bCheck, Balloon bCurrent)
        {
            if (bCheck.Strength > bCurrent.Strength)
                return true;
            return false;
        }
        public bool WeakestBallonCheck(Balloon bCheck, Balloon bCurrent)
        {
            if (bCheck.Strength < bCurrent.Strength)
                return true;
            return false;
        }
        #endregion

        /// <summary>
        /// Bestimmt alle Pfadsegmente die in der Reichweite des Turmes sind
        /// </summary>
        /// <param name="map"></param>
        public void FindSegmentsInRange(Map map)
        {
            List<int> segments = new List<int>();
            for (int i = 0; i < map.BalloonPath.Length - 1; i++)
            {
                Vector2D locationP = new Vector2D(map.BalloonPath[i].X, map.BalloonPath[i].Y);
                Vector2D directionP = new Vector2D(map.BalloonPath[i + 1].X - locationP.X, map.BalloonPath[i + 1].Y - locationP.Y);
                Vector2D centreP = new Vector2D(Hitbox.X + Hitbox.Width / 2, Hitbox.Y + Hitbox.Height / 2);
                float factor = -1 * (directionP.X * (locationP.X - centreP.X) + directionP.Y * (locationP.Y - centreP.Y)) / (directionP.X * directionP.X + directionP.Y * directionP.Y);
                if (factor > 1)
                    factor = 1;
                else if (factor < 0)
                    factor = 0;

                Vector2D closestP = locationP + factor * directionP;
                if ((closestP - centreP).Magnitude <= Range)
                    segments.Add(i);
            }
            SegmentsInRange = segments;
        }
    }
}
