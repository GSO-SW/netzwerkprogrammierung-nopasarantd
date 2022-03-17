using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NoPasaranTD.Data
{
    public static class StaticInfo
    {

        #region Startwerte
        public static int StartMoney = 1500;
        public static int StartHP = 100;
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
            return name; ;
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

        #region GetBalloon Methoden 
        public static readonly Size BalloonSize = new Size(10, 10);

        /// <summary>
        /// Öffentliche Methoden zum Aufrufen der BallonDaten/Werte 
        /// </summary>
        public static uint GetBalloonStrength(BalloonType type)
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

        #region Obstacle
        private static readonly Dictionary<Type, Bitmap> ObstacleImage = new Dictionary<Type, Bitmap>() { };

        private static readonly Dictionary<Type, Size> ObstacleSize = new Dictionary<Type, Size>() { };
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
        private static readonly Dictionary<Type, uint> TowerPrice = new Dictionary<Type, uint>()
        {
            {typeof(TowerCanon),     40},
            {typeof(TowerArtillerie), 230},
        };

        private static readonly Dictionary<Type, uint> TowerDamage = new Dictionary<Type, uint>()
        {
            {typeof(TowerCanon),     1},
            {typeof(TowerArtillerie), 8},
        };

        private static readonly Dictionary<Type, double> TowerRange = new Dictionary<Type, double>()
        {
            {typeof(TowerCanon),     450},
            {typeof(TowerArtillerie), 1000},
        };

        private static readonly Dictionary<Type, Size> TowerSize = new Dictionary<Type, Size>()
        {
            {typeof(TowerCanon),     new Size(50, 50)},
            {typeof(TowerArtillerie), new Size(150, 80)},
        };

        private static readonly Dictionary<Type, uint> TowerDelay = new Dictionary<Type, uint>()
        {
            {typeof(TowerCanon),     2500},
            {typeof(TowerArtillerie), 48000},
        };
        #endregion // Dictionary für die Türme
    }
}