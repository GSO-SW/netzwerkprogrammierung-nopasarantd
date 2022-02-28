using NoPasaranTD.Model;
using NoPasaranTD.Utilities;
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

		/// <summary>
		/// Überprüft, ob das jeweilige Objekt an der bestimmten Stelle platziert werden kann, oder ob es eine Kollision gibt
		/// </summary>
		/// <param name="rect">Rechteck das zur Kollision überprüft wird</param>
		/// <returns></returns>
		public bool IsTowerValidPosition(Rectangle rect)
        {
            foreach (var item in Towers)                       //Überprüft, ob das Rechteck mit den Towern kollidiert
            {
                if (item.Hitbox.IntersectsWith(rect))
                {
					return false;
                }
            }

            foreach (var item in CurrentMap.Obstacles) //Überprüft, ob das Rechteck mit den Hindernisssen kollidiert
            {
				if (item.Hitbox.IntersectsWith(rect))
				{
					return false;
				}
			}
            if (!TowerCollisionPath(rect))  //Wenn Kollision mit dem Path, dann wird false zurückgegeben
            {
				return false;
            }
            return true;
        }
	}
}
