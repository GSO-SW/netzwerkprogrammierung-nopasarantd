using NoPasaranTD.Model;
using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
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

		/// <summary>
		/// Gibt einen Ballon in Reichweite des Turms zurück der am weitesten ist
		/// </summary>
		/// <param name="tower"></param>
		/// <returns>Index des Ziels in der Liste Balloons.</br>
		/// Ohne Ballon in Reichweite -1</returns>
		public int TowerTarget(int tower)
        {
			List<int> ballonsInRange = new List<int>();
			// Alle Ballons in der Reichweite des Turms bestimmen
            for (int i = 0; i < Balloons.Count; i++)
            {
				Vector2D currentPosition = CurrentMap.GetPathPosition(Balloons[i].PathPosition); // Position des Ballons
				Vector2D towerCentre = new Vector2D(Towers[tower].Hitbox.Location.X + Towers[tower].Hitbox.Width, Towers[tower].Hitbox.Location.Y + Towers[tower].Hitbox.Height); // Zentrale Position des Turmes
				if ((currentPosition - towerCentre).Magnitude <= Towers[tower].Range) //Länge des Verbindungsvektors zwischen Turmmitte und dem Ballon muss kleiner sein als der Radius des Turmes
					ballonsInRange.Add(i);
            }
            if (ballonsInRange.Count == 0) // Sollte kein Ballon in der Reichweite sein
				return -1;

			int farthestIndex = 0;
			for (int i = 0; i < ballonsInRange.Count; i++) // Alle Ballons im Radius checken welcher am weitesten ist
                if (Balloons[ballonsInRange[i]].PathPosition > Balloons[ballonsInRange[farthestIndex]].PathPosition)
					farthestIndex = i;
			return farthestIndex;
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
	}
}
