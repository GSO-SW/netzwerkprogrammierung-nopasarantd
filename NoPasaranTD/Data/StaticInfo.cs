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
        static int i = 1;


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

    }
}
