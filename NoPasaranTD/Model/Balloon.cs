using NoPasaranTD.Data;
using System;

namespace NoPasaranTD.Model
{
    /// <summary>
    /// Model Klasse eines Ballon Objektes
    /// </summary>
    [Serializable]
    public class Balloon
    {
        public Balloon(BalloonType type)
        {
            Type = type;
        }

        /// <summary>
        /// Der Ballon Type mithilfe dessen weitere Memberwerte bestimmt werden
        /// </summary>
        public BalloonType Type { get; set; } = BalloonType.None;

        /// <summary>
        /// Die derzeitige Position des Ballones auf dem Pfad in Prozentangabe
        /// </summary>
        public float PathPosition { get; set; }

        /// <summary>
        /// Die Stärke des Ballones (Abhängig vom Ballon Typen)
        /// </summary>
        public uint Strength => StaticInfo.GetBalloonStrength(Type);

        /// <summary>
        /// Das Geld welches dieses Ballon einbringt, sobald es zerstört wurde (Abhängig vom Ballon Typen)
        /// </summary>
        public uint Value { get => StaticInfo.GetBalloonValue(Type); }

        /// <summary>
        /// Das Pfadsegment auf dem sich der Ballon zu dem Zeitpunkt befindet
        /// </summary>
        public uint CurrentSegment { get; set; } = 0;
    }

    /// <summary>
    /// Typen eines Ballones
    /// </summary>
    [Serializable]
    public enum BalloonType
    {
        None,
        Red,
        Green,
        Blue,
        Purple,
        Black,
        Gold,
    }
}
