namespace NoPasaranTD.Model
{
    /// <summary>
    /// Model Klasse eines Ballon Objektes
    /// </summary>
    public class Balloon
    {
        public Balloon(BalloonType type)
        {
            ballonType = type;
        }
      
        public Balloon() { }
        
        private BalloonType ballonType = BalloonType.None;
        /// <summary>
        /// Der Ballon Type mithilfe dessen weitere Memberwerte bestimmt werden
        /// </summary>
        public BalloonType Type { get => ballonType; set => ballonType = value; }

        /// <summary>
        /// Die derzeitige Position des Ballones auf dem Pfad in Prozentangabe
        /// </summary>
        public uint PathPosition { get; set; }

        /// <summary>
        /// Die Stärke des Ballones (Abhängig vom Ballon Typen)
        /// </summary>
        public uint Strength { get /* Zurgiff auf StaticInfo */; }

    }

    /// <summary>
    /// Typen eines Ballones
    /// </summary>
    public enum BalloonType
    {
        None,
        Rot,
        Blau,
        Grün,
        Lila,
        Schawrz,
        Gold,
    }
}
