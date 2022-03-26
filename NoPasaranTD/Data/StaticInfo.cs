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
        public static int StartMoney = 150;
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
        #endregion // Dictionary für die Türme

        #region Wellenwerte

        private static Dictionary<BalloonType, uint> PeekRoundBallon = new Dictionary<BalloonType, uint>()
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

        public static List<string> DichterUndDenker = new List<string>()
        {
            "Schafe sind wollige Tiere!",
            "Stifte sind gut für die Nase!",
            "Die Haut abziehen und an die Wand kleben! ~ Louis 21.02.2021",
            "KEIN DURCHKOMMEN!!!!",
            "Warum liegt hier Stoh?",
            "Warum hast du eine Maske auf?",
            "Och ne, mein Taschenrechner hat sich verrechnet! ~ Alex 19.02.2022",
            "Das sind dann auch noch so junge und knackige Nieren ~ Lars 16.03.22",
            "Kartoffelgüraum ~ Alex 17.01.22",
            "Ich bin Designer! ~ Alex 16.12.2021",
            "Was ist eine politische Rede ohne Meef?",
            "Todays loss is tomorrows sausage ~ Lars 2020",
            "π = 5!",
            "Jetzt kannst du dir bei deiner fetten Mudda ein Lolli abholen! ~ Alex",
            "Ich höre eine Ampel :X ~ Alexander",
            "Drogenspürhund V. aka Paolo",
            "E E EEEEEEEEE!!!",
            "Yeesus",
            "Lass mal nach Milfs! ~ Lars 05.08.2021",
            "Ich kenne noch keinen Italiener der noch keine Kaffeemaschiene gefickt hat ~ Paolo 17.03.2022",
            "Wenn du im Auto pennst, dann kannst du dir die Rente sparen ~ Lars 16.11.2021",
            "Ich ficke jetzt voll in meinen Bildschirm rein ~ Alexander 14.03.22",
            "Manchmal Hocke ich mich auf meine Stufe und kacke ins Aquarium ~ Alexander 27.02.2022",
            "Poseidon, irgendso ein scheiss Franzose ~ Paolo 15.11.21",
            "Manchmal höre ich Alex scheissen ~ Louis 22.01.2022",
            "Oh no, erst einmal in die Hosen scheißen AUS PROTEST! ~ Paolo 26.01.2022",
            "MEEF! ~ Lars & Jakob",
            "Alexander Patola, Erster seines Namen, Vorhautmensch, Ehren collector, Befreier der Vorhäute, Pickpocketmaster, Religionsgrund, vom Königreich Polen und nehmer der Vorhaut aller Lebewesen.",
            "Wenn dein Stealth Level hoch genug ist, dann kannst du jemandem die Vorhaut nehmen. Pickpocket 100 ~ ~ Julian Februar 2021",
            "Ich verkaufe Kinder! ~ Edgar",
            "Silikongefüllte Brüste sind irgendwie unnötig, warum packt man da keine Kaffeemaschine rein oder so ~ Edgar 2020",
            "Ich habe einen Anschlag auf Sie vor! ~ KF",
            "Lösen Sie dies mithilfe von GMV! (gesunder Menschenverstand) ~ KF",
            "Die Rituale beginnen immer Freitags um 16:00 und enden Montags um 08:00 Uhr! ~ JP",
            "Ich habe extra Möhren mitgebracht um das beste Klausurfeeling für Sie zu erzeugen ~ JP",
            "Wenn du stirbst bist du Tod!",
            "Ballons tun dir Weh!",
            "Wenn du Obdachloss bist, kauf dir doch einfach ein Haus!",
            "Geringverdiener!",
            "Der Mark(t) regelt!. Ausser Benzinpreise natürlich!",
            "Mission 5 Points!",
            "Schreibst du 4 bleibst du hier!",
        };

        #endregion
    }
}