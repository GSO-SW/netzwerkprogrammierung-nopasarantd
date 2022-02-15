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
        public uint Strength { get; }
        public uint Speed { get; }
        public uint Range { get; }
        public abstract void Render();
        public abstract void Update();
    }
}
