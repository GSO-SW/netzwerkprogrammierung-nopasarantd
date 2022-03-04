using NoPasaranTD.Model;
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

        #region Startwerte
        public static int StartMoney = 150;
        public static int StartHP = 100;
        public static int PathWidth = 24;
        #endregion

        #region Obstacle
        private static readonly Dictionary<Type, Bitmap> ObstacleImage = new Dictionary<Type, Bitmap>()
        {

        };
        
        private static readonly Dictionary<Type, Size> ObstacleSize = new Dictionary<Type, Size>()
        {

        };

        
        #endregion

        #region GetObstacle Methoden

        public static Bitmap GetObstacleImage(Type type)
        {
            ObstacleImage.TryGetValue(type, out Bitmap obstacleimg);
            return obstacleimg;
        }

        public static Size GetObstacleSize(Type type)
        {
            ObstacleSize.TryGetValue(type, out Size obstaclesize);
            return obstaclesize;
        }
        #endregion

        #region GetTower Methoden

        /// <summary>
        /// Öffentliche Methoden zum Aufrufen der TowerDaten/Werte 
        /// </summary>
        public static Bitmap GetTowerImage(Type type)
        {
            TowerImage.TryGetValue(type, out Bitmap towerimg);
            return towerimg;
        }

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

        #region GetBallon Methoden 

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

        private static readonly Dictionary<Type, Bitmap> TowerImage = new Dictionary<Type, Bitmap>()
        {

        };

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
            //{typeof(TowerCanon),     1},
        };

        private static readonly Dictionary<Type, double> TowerRange = new Dictionary<Type, double>()
        {
            {typeof(TowerTest),     150},
        };

        private static readonly Dictionary<Type, Size> TowerSize = new Dictionary<Type, Size>()
        {
            //{typeof(TowerCanon),     new Size(1, 1)},
        };

        private static readonly Dictionary<Type, uint> TowerDelay = new Dictionary<Type, uint>()
        {
            {typeof(TowerTest),     250},
        };
        #endregion // Dictionary für die Türme
    }
}