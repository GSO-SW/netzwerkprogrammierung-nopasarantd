﻿using NoPasaranTD.Data;

namespace NoPasaranTD.Model
{
    /// <summary>
    /// Model Klasse eines Ballon Objektes
    /// </summary>
    public class Balloon
    {
        public Balloon(BalloonType type)
        {
            Type = type;
        }
      
        public Balloon() { }
        
        /// <summary>
        /// Der Ballon Type mithilfe dessen weitere Memberwerte bestimmt werden
        /// </summary>
        public BalloonType Type { get; set; } = BalloonType.None;

        /// <summary>
        /// Die derzeitige Position des Ballones auf dem Pfad in Prozentangabe
        /// </summary>
        public float PathPosition { get; set; }

    }

    /// <summary>
    /// Typen eines Ballones
    /// </summary>
    public enum BalloonType
    {
        None,
        Red,
        Blue,
        Green,
        Purple,
        Black,
        Gold,
    }
}
