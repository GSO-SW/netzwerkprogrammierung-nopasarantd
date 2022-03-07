using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public Func<Balloon, Balloon, bool> GetBallonFunc { get; set; }


        public abstract void Render(Graphics g);
        public abstract void Update(Game game, int TowerIndex);

        #region BallonChecks
        /// <summary>
        /// Kontrolliert, ob der Ballon Check weiter auf dem Pfad ist als der Ballon bCurrent
        /// </summary>
        /// <param name="bCheck"></param>
        /// <param name="bCurrent"></param>
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
    }
}
