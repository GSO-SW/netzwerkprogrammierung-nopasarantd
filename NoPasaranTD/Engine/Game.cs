using NoPasaranTD.Data;
using NoPasaranTD.Engine.Visuals;
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
	public class Game
	{
		int tickCount;
		public Map CurrentMap { get; }
		public List<Balloon> Balloons { get; }
		public List<Tower> Towers { get; }
		public UILayout UILayout { get; private set; }


		public Game(Map map)
		{
			CurrentMap = map;
			Towers = new List<Tower>();
			Balloons = new List<Balloon>();
			Engine.OnRender += Render;
			Engine.OnUpdate += Update;
			UILayout = new UILayout(this);
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
			return ballonsInRange[farthestIndex];
        }

		public void Update()
		{
			if (tickCount == 1000)
			{
				tickCount = 0;
				Balloons.Add(new Balloon() { PathPosition = 0, Type = BalloonType.Black });
            }
			tickCount++;

			for (int i = 0; i < Towers.Count; i++)
			{
				int target = TowerTarget(i);
				Towers[i].IncreaseCoolDownTick();
				if (target != -1)
                {
                    if (Towers[i].ShootRequest())
                    {
						Balloons[target].Type -= 1;
						if (Balloons[target].Type == BalloonType.None)
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
			Brush brush = new SolidBrush(Color.Blue);
			for (int i = 0; i < CurrentMap.BalloonPath.Length - 1; i++)
			{
				g.DrawLine(pen, new PointF(CurrentMap.BalloonPath[i].X, CurrentMap.BalloonPath[i].Y), new PointF(CurrentMap.BalloonPath[i + 1].X, CurrentMap.BalloonPath[i + 1].Y));
			}
			foreach (var item in Balloons)
			{
                switch (item.Type)
                {
					case BalloonType.Red: brush = new SolidBrush(Color.Red); break;
					case BalloonType.Blue: brush = new SolidBrush(Color.Blue); break;
					case BalloonType.Green: brush = new SolidBrush(Color.Green); break;
					case BalloonType.Purple: brush = new SolidBrush(Color.Purple); break;
					case BalloonType.Black: brush = new SolidBrush(Color.Black); break;
				}
				Vector2D position = CurrentMap.GetPathPosition(item.PathPosition);
				g.FillEllipse(brush, new RectangleF(new PointF(position.X - StaticInfo.GetBalloonSize.Width / 2, position.Y - StaticInfo.GetBalloonSize.Height / 2), StaticInfo.GetBalloonSize));
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

		/// <summary>
		/// Kontrolliert, ob das angegebene Recheck einen Abstand von 50 Einheiten zum nächsten Pfad hat 
		/// </summary>
		/// <param name="rect">Zu überprüfendes Rechteck</param>
		/// <returns>False wenn es eine Überschneidung gibt</returns>
		public bool TowerCollisionPath(Rectangle rect)
        {
            for (int i = 0; i < CurrentMap.BalloonPath.Length - 1; i++) // Alle Pfadteile durchgehen
            {
				Vector2D positionV = CurrentMap.BalloonPath[i]; // Ortsvektor der Gerade die den Pfadabschnitt darstellt
				Vector2D connectionV = CurrentMap.BalloonPath[i + 1] - CurrentMap.BalloonPath[i]; // Verbindungsvektor der Gerade die den Pfadabschnitt darstellt
                for (int j = 0; j < 2; j++) // Durchgehen beider X Möglichkeiten der verschiebung vom Eckpunkt oben links
                {
                    for (int k = 0; k < 2; k++) // Durchgehen beider Y Möglichkeiten
                    {
						Vector2D rectangleCornerV = new Vector2D(rect.X + j * rect.Width, rect.Y + k * rect.Height); // Verschiebung des Eckpunktes zur kontrolle
						// Lotfußpunktverfahren zu einer Formel umgestellt
						float closestPointDistance = -1 * (((positionV.X - rectangleCornerV.X) * connectionV.X + (positionV.Y - rectangleCornerV.Y) * connectionV.Y) / ((connectionV.X * connectionV.X) + (connectionV.Y * connectionV.Y)));
						//Bestimmen des Punkts auf dem Pfad der am nächsten am Eckpunkt ist
						Vector2D closestV = new Vector2D(positionV.X + closestPointDistance * connectionV.X, positionV.Y + closestPointDistance * connectionV.Y);

						// Länge der kürzesten Verbindung bestimmen
						if ((closestV - rectangleCornerV).Magnitude < 50) // TODO: Wert an StaticInfo festmachen
                        {
							return false;
                        }
                    }
                }
               
            }
			return true;
        }
	}
}
