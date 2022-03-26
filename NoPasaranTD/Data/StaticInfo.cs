using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace NoPasaranTD.Data
{
    public static class StaticInfo
    {

        #region Startwerte
        public static int StartMoney = 150;
        public static int StartHP = 100;
        #endregion

        #region GetObstacle Methoden
        public static Bitmap GetObstacleImage(ObstacleType type)
        {
            ObstacleImage.TryGetValue(type, out Bitmap obstacleimg);
            return obstacleimg;
        }
        #endregion

        #region GetTower Methoden
        /// <summary>
        /// Öffentliche Methoden zum Aufrufen der TowerDaten/Werte 
        /// </summary>
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

        public static uint GetTowerUpgradePrice(Type type)
        {
            TowerUpgradePrice.TryGetValue(type, out uint price);
            return price;
        }

        public static uint GetTowerLevelCap(Type type)
        {
            TowerLevelCap.TryGetValue(type, out uint levelCap);
            return levelCap;
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
        private static readonly Dictionary<ObstacleType, Bitmap> ObstacleImage = new Dictionary<ObstacleType, Bitmap>()
        {
            {ObstacleType.Pool, ResourceLoader.LoadBitmapResource("NoPasaranTD.Resources.pool.png") },
            {ObstacleType.Factory, ResourceLoader.LoadBitmapResource("NoPasaranTD.Resources.factory.png" )}
        };
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
            {typeof(TowerArtillery), 230},
        };

        private static readonly Dictionary<Type, uint> TowerDamage = new Dictionary<Type, uint>()
        {
            {typeof(TowerCanon),     1},
            {typeof(TowerArtillery), 8},
        };

        private static readonly Dictionary<Type, double> TowerRange = new Dictionary<Type, double>()
        {
            {typeof(TowerCanon),     450},
            {typeof(TowerArtillery), 1000},
        };

        private static readonly Dictionary<Type, Size> TowerSize = new Dictionary<Type, Size>()
        {
            {typeof(TowerCanon),     new Size(50, 50)},
            {typeof(TowerArtillery), new Size(150, 80)},
        };

        private static readonly Dictionary<Type, uint> TowerDelay = new Dictionary<Type, uint>()
        {
            {typeof(TowerCanon),     2500},
            {typeof(TowerArtillery), 48000},
        };

        private static readonly Dictionary<Type, uint> TowerUpgradePrice = new Dictionary<Type, uint>()
        {
            {typeof(TowerCanon),     30},
            {typeof(TowerArtillery), 200},
        };

        private static readonly Dictionary<Type, uint> TowerLevelCap = new Dictionary<Type, uint>()
        {
            {typeof(TowerCanon),     6},
            {typeof(TowerArtillery), 3},
        };
        public static readonly List<Image> TowerCanonImages = new List<Image>()
        {
            new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("NoPasaranTD.Resources.Canon_Body.png")),
            new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("NoPasaranTD.Resources.Canon_Head.png")),
            new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("NoPasaranTD.Resources.Canon_Barrel.png"))
        };
        #endregion // Dictionary für die Türme

        #region Wellenwerte

        private static readonly Dictionary<BalloonType, uint> PeekRoundBallon = new Dictionary<BalloonType, uint>()
        {
            {BalloonType.Red,0},
            {BalloonType.Green,10},
            {BalloonType.Blue,20},
            {BalloonType.Purple,30},
            {BalloonType.Black,40 },
            {BalloonType.Gold,50 },
        };

        public static uint GetBallonPeek(BalloonType type)
        {
            PeekRoundBallon.TryGetValue(type, out uint result);
            return result;
        }

        #endregion
    }
}