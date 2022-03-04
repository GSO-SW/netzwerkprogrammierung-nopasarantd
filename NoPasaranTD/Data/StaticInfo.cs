using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Data
{
    public static class StaticInfo
    {
        

        #region GetTower methoden

        /// <summary>
        /// Öffentliche Methoden zum Aufrufen der TowerDaten/Werte 
        /// </summary>
        
        public static string GetTowerName(Type type)
        {
            TowerName.TryGetValue(type, out string name);
            return name;
        }
        public static uint GetTowerPrice(Type type)
        {
            TowerPrice.TryGetValue(type, out uint price);
            return price;
        }

        public static uint GetTowerDamage(Type type)
        {
            TowerDamage.TryGetValue(type, out uint damage);
            return damage;
        }
        public static double GetTowerRange(Type type)
        {
            TowerRange.TryGetValue(type, out double range);
            return range;
        }
        public static Size GetTowerSize(Type type)
        {
            TowerSize.TryGetValue(type, out Size size);
            return size;
        }
        public static uint GetTowerDelay(Type type)
        {
            TowerDelay.TryGetValue(type, out uint delay);
            return delay;
        }
        #endregion

        #region GetBallon methoden 

        public static Size GetBalloonSize = new Size(10,10);

        /// <summary>
        /// Öffentliche Methoden zum Aufrufen der BallonDaten/Werte 
        /// </summary>
        public static uint GetBallonStrength(BalloonType type)
        {
            BalloonStrength.TryGetValue(type, out uint strength);
            return strength;
        }
        public static float GetBalloonVelocity(BalloonType type)
        {
            BalloonVelocity.TryGetValue(type, out float velocity);
            return velocity;
        }
        public static uint GetBalloonValue(BalloonType type)
        {
            BalloonValue.TryGetValue(type, out uint value);
            return value;
        }
        #endregion

        #region Ballon
        private static readonly Dictionary<BalloonType, uint> BalloonStrength = new Dictionary<BalloonType, uint>()
        {
            {BalloonType.None,     0},
            {BalloonType.Red,      1},
            {BalloonType.Green,    2},
            {BalloonType.Blue,     3},
            {BalloonType.Purple,   4},
            {BalloonType.Black,    5},
            {BalloonType.Gold,     6}
        };

        private static readonly Dictionary<BalloonType, float> BalloonVelocity = new Dictionary<BalloonType, float>()
        {
            {BalloonType.None,     0},
            {BalloonType.Red,      1},
            {BalloonType.Green,    2},
            {BalloonType.Blue,     3},
            {BalloonType.Purple,   4},
            {BalloonType.Black,    5},
            {BalloonType.Gold,     6}
        };

        private static readonly Dictionary<BalloonType, uint> BalloonValue = new Dictionary<BalloonType, uint>()
        {
            {BalloonType.None,     0},
            {BalloonType.Red,      1},
            {BalloonType.Green,    2},
            {BalloonType.Blue,     3},
            {BalloonType.Purple,   4},
            {BalloonType.Black,    5},
            {BalloonType.Gold,     6}
        };
        #endregion  // Dictionary für die Ballons

        #region Tower
        private static readonly Dictionary<Type, string> TowerName = new Dictionary<Type, string>()
        {
            //{typeof(TowerCanon),     "Canon"},
        };

        private static readonly Dictionary<Type, uint> TowerPrice = new Dictionary<Type, uint>()
        {
            //{typeof(TowerCanon),     40},
        };

        private static readonly Dictionary<Type, uint> TowerDamage = new Dictionary<Type, uint>()
        {
            {typeof(TowerCanon),     1},
        };

        private static readonly Dictionary<Type, double> TowerRange = new Dictionary<Type, double>()
        {
            {typeof(TowerCanon),     150},
        };

        private static readonly Dictionary<Type, Size> TowerSize = new Dictionary<Type, Size>()
        {
            {typeof(TowerCanon),     new Size(50, 50)},
        };

        private static readonly Dictionary<Type, uint> TowerDelay = new Dictionary<Type, uint>()
        {
            {typeof(TowerCanon),     2500},
        };
        #endregion // Dictionary für die Türme
    }
}