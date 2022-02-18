using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Model
{
    public abstract class Tower
    {
        public Rectangle Hitbox { get; set; }
        public uint Level { get; set; }
        public uint Strength { get /* TODO: Zurgiff auf StaticInfo */; }
        public uint Speed { get /* TODO: Zurgiff auf StaticInfo */; }
        public uint Range { get /* TODO: Zurgiff auf StaticInfo */; }
        
        public abstract void Render(Graphics g);
        public abstract void Update();
    }
}
