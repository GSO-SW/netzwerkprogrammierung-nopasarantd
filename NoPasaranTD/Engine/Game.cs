﻿using NoPasaranTD.Data;
using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Engine
{
	public class Game
	{
		int tickCount;
		public Map CurrentMap { get; }
		public List<Balloon> Balloons { get; }
		public List<Tower> Towers { get; }

		public Game(Map map)
		{
			CurrentMap = map;
			Towers = new List<Tower>();
			Balloons = new List<Balloon>();
			Engine.OnRender += Render;
			Engine.OnUpdate += Update;

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
				Vector2D towerCentre = new Vector2D(Towers[tower].Hitbox.Location.X + Towers[tower].Hitbox.Width / 2, Towers[tower].Hitbox.Location.Y + Towers[tower].Hitbox.Height / 2); // Zentrale Position des Turmes
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
			if (tickCount == 1000)
			{
				tickCount = 0;
				Balloons.Add(new Balloon() { PathPosition = 0, Type = BalloonType.Blue });
            }
			tickCount++;
			for (int i = 0; i < Towers.Count; i++)
			{
				int target = TowerTarget(i);
				if (target != -1 && Towers[i].ShootRequest())
                {
					Balloons[target].Type -= 1;
					if (Balloons[target].Type == BalloonType.None)
					{
						Balloons.RemoveAt(target);
					}
                }
			}

			for (int i = 0; i < Balloons.Count; i++)
            {
				Balloons[i].PathPosition += 0.2f; // TODO get speed
                if (Balloons[i].PathPosition >= CurrentMap.PathLength)
                {
					Balloons.RemoveAt(i);
					i = 0;
                }
            }
				
		}

		public void Render(Graphics g)
        {
			Pen pen = new Pen(Color.Black);
			Brush brushRed = new SolidBrush(Color.Blue);
			for (int i = 0; i < CurrentMap.BalloonPath.Length - 1; i++)
			{
				g.DrawLine(pen, new PointF(CurrentMap.BalloonPath[i].X, CurrentMap.BalloonPath[i].Y), new PointF(CurrentMap.BalloonPath[i + 1].X, CurrentMap.BalloonPath[i + 1].Y));
			}
			foreach (var item in Balloons)
			{
				Vector2D position = CurrentMap.GetPathPosition(item.PathPosition);
				g.FillEllipse(brushRed, new RectangleF(new PointF(position.X - StaticInfo.GetBalloonSize.Width / 2, position.Y - StaticInfo.GetBalloonSize.Height / 2), StaticInfo.GetBalloonSize));
			}
            foreach (var item in Towers)
            {
				Vector2D towerCentre = new Vector2D(item.Hitbox.Location.X + item.Hitbox.Width / 2, item.Hitbox.Location.Y + item.Hitbox.Height / 2);
				g.DrawEllipse(pen, new RectangleF(new PointF(towerCentre.X - item.Range,towerCentre.Y - item.Range), new SizeF(item.Range * 2, item.Range * 2)));
            }
            
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
