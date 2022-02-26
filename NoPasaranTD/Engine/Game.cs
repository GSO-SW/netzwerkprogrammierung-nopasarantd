using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Engine
{
	class Game
	{
		public Map CurrentMap { get; }
		public List<Balloon> Balloons { get; }
		public List<Tower> Towers { get; }

		public Game(Map map)
		{
			CurrentMap = map;
			Towers = new List<Tower>();
			Balloons = new List<Balloon>();
		}

		public void Update()
		{
			for (int i = 0; i < Towers.Count; i++)
				Towers[i].Update();
			for (int i = 0; i < Balloons.Count; i++)
				Balloons[i].PathPosition += 1f; // TODO get speed
		}
		public void AddTower(Tower t)
		{
			// TODO network communication
			Towers.Add(t);
		}
		public void RemoveTower(Tower t)
		{
			// TODO network communication
			Towers.Remove(t);
		}
		public bool IsTowerValidPosition(Rectangle rect)
        {
            foreach (var item in Towers)
            {
                if (item.Hitbox.IntersectsWith(rect))
                {
					return false;
                }
            }

            foreach (var item in CurrentMap.Obstacles)
            {
				if (item.Hitbox.IntersectsWith(rect))
				{
					return false;
				}
			}
            return true;
        }
	}
}
