﻿using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using System.Drawing;

namespace NoPasaranTD.Model
{
    public abstract class Tower
    {
        public Rectangle Hitbox { get; set; }
        public uint Level { get; set; }

        public uint Strength { get => StaticInfo.GetTowerDelay(GetType()); }
        public uint Delay { get => StaticInfo.GetTowerDelay(GetType()); }
        public double Range { get => StaticInfo.GetTowerRange(GetType()); }
        
        public abstract void Render(Graphics g);
        public abstract void Update(Game game, int targetIndex);
    }
}
