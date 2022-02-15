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

        private static Dictionary<BalloonType, int> BalloonStrength = new Dictionary<BalloonType, int>()
        {
            {BalloonType.None,     0},
            {BalloonType.Red,      1},
            {BalloonType.Green,    2},
            {BalloonType.Blue,     3},
            {BalloonType.Purple,   4},
            {BalloonType.Black,    5},
            {BalloonType.Gold,     6}
        };

    }
}
