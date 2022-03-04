using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using System.Diagnostics;
using System.Drawing;

namespace NoPasaranTD.Model
{
    public abstract class Tower
    {
        public Rectangle Hitbox { get; set; } // TODO should size of rectangle be accessable?
        public uint Level { get; set; }

        public uint Strength { get => StaticInfo.GetTowerDamage(GetType()); }
        public uint Delay { get => StaticInfo.GetTowerDelay(GetType()); }
        public double Range { get => StaticInfo.GetTowerRange(GetType()); }
        
        public abstract void Render(Graphics g);
        public abstract void Update(Game game, int targetIndex);
    }

    
}
