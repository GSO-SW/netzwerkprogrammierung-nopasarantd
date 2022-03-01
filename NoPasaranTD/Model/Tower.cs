using NoPasaranTD.Data;
using NoPasaranTD.Engine;
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
        public uint Strength { get => StaticInfo.GetTowerDelay(typeof(Tower)); } // TODO: Spezifizieren
        public uint Speed { get => StaticInfo.GetTowerDelay(typeof(Tower)); }
        public uint Range { get => (uint)StaticInfo.GetTowerRange(typeof(Tower)); }
        
        public abstract void Render(Graphics g);
        public abstract void Update(Game game, int targetIndex);
    }
}
