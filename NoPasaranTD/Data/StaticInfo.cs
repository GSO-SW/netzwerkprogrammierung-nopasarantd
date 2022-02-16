using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Data
{
    public static class StaticInfo
    {
        private static readonly int i = 1;

        #region Ballon
        private static readonly Dictionary<BalloonType, int> BalloonStrength = new Dictionary<BalloonType, int>()
        {
            {BalloonType.None,     0},
            {BalloonType.Red,      1},
            {BalloonType.Green,    2},
            {BalloonType.Blue,     3},
            {BalloonType.Purple,   4},
            {BalloonType.Black,    5},
            {BalloonType.Gold,     6}
        };

        private static readonly Dictionary<BalloonType, int> BalloonVelocity = new Dictionary<BalloonType, int>()
        {
            {BalloonType.None,     0},
            {BalloonType.Red,      i},
            {BalloonType.Green,    i*2},
            {BalloonType.Blue,     i*3},
            {BalloonType.Purple,   i*4},
            {BalloonType.Black,    i*5},
            {BalloonType.Gold,     i*6}
        };

        private static readonly Dictionary<BalloonType, int> BalloonValue = new Dictionary<BalloonType, int>()
        {
            {BalloonType.None,     0},
            {BalloonType.Red,      i},
            {BalloonType.Green,    i*2},
            {BalloonType.Blue,     i*3},
            {BalloonType.Purple,   i*4},
            {BalloonType.Black,    i*5},
            {BalloonType.Gold,     i*6}
        };
        #endregion  // Dictionary für die Ballons

        #region Tower
        private static readonly Dictionary<TowerType, string> TowerName = new Dictionary<TowerType, string>()
        {
            {TowerType.Canon,     "Canon"},
        };
        
        private static readonly Dictionary<TowerType, double> TowerCost = new Dictionary<TowerType, double>()
        {
            {TowerType.Canon,     40},
        };

        private static readonly Dictionary<TowerType, double> TowerDamage = new Dictionary<TowerType, double>()
        {
            {TowerType.Canon,     1},
        };

        private static readonly Dictionary<TowerType, double> TowerRange = new Dictionary<TowerType, double>()
        {
            {TowerType.Canon,     10},
        };

        private static readonly Dictionary<TowerType, string> TowerDamagetyp = new Dictionary<TowerType, string>()
        {
            {TowerType.Canon,     "AOE"},
        };

        private static readonly Dictionary<TowerType, double> TowerSize = new Dictionary<TowerType, double>()
        {
            {TowerType.Canon,     1*1},
        };

        private static readonly Dictionary<TowerType, double> TowerAttackspeed = new Dictionary<TowerType, double>()
        {
            {TowerType.Canon,     3},
        };
        #endregion // Dictionary für die Türme
    }
}
