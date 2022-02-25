using NoPasaranTD.Data;
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
        private uint coolDownTick;
        public bool ShootRequest()
        {
            if (coolDownTick >= Speed)
            {
                coolDownTick = 0;
                return true;
            }
            return false;
        }

        public void IncreaseCoolDownTick()
        {
            if (coolDownTick < Speed)
                coolDownTick++;
        }

        public Rectangle Hitbox { get; set; }
        public uint Level { get; set; }
        public uint Strength { get /* TODO: Zurgiff auf StaticInfo */; }
        public uint Speed { get => StaticInfo.GetTowerDelay(typeof(TowerTest)); }
        public uint Range { get => (uint)StaticInfo.GetTowerRange(typeof(TowerTest)); }
        
        public abstract void Render(Graphics g);
        public abstract void Update();
    }
}
