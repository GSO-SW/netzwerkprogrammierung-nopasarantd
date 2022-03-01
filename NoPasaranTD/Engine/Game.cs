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
		private readonly Random random = new Random();

		private uint currentTick; // TODO: Zu alle referenzen zu Servertick ändern

		public Map CurrentMap { get; }
		public List<Balloon> Balloons { get; }
		public List<Tower> Towers { get; }

		public Game(Map map)
		{
			CurrentMap = map;
			Towers = new List<Tower>();
			Balloons = new List<Balloon>();
			Engine.OnUpdate += Update;
		}

		public void Update()
		{
			for (int i = 0; i < Towers.Count; i++)
				Towers[i].Update();
			for (int i = 0; i < Balloons.Count; i++)
				Balloons[i].PathPosition += 1f; // TODO get speed

			ManageBalloonSpawn();
			currentTick++; // Emuliere Servertick
		}

		private void ManageBalloonSpawn()
		{
			if (currentTick % 1000 == 0)
			{ // Spawne jede Sekunde einen Ballon
				Balloon balloon = new Balloon
				{
					PathPosition = 0
				};

				BalloonType[] values = (BalloonType[])Enum.GetValues(typeof(BalloonType));
				balloon.Type = values[random.Next(0, values.Length - 1)];
				Balloons.Add(balloon);
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
		/// Kontrolliert, ob das angegebene Recheck einen Abstand von X Einheiten zum nächsten Pfadpunkt hat und ob keine Hitbox des Pfades getroffen wurde
		/// </summary>
		/// <param name="rect">Zu überprüfendes Rechteck</param>
		/// <returns>False wenn es eine Überschneidung gibt</returns>
		public bool TowerCollisionPath(Rectangle rect)
		{
			Vector2D[] cornersV = new Vector2D[4]; // Speichern der Ecken des Rechtecks
			for (int i = 0; i < 2; i++) // Alle Ecken durchgehen
				for (int j = 0; j < 2; j++)
					cornersV[i * 2 + j] = new Vector2D(rect.X + i * rect.Width, rect.Y + j * rect.Height); // Ecken abspeichern
			Vector2D save = cornersV[2]; // Ecke 2 mit 3 tauschen um eine durchgehende Reihenfolge zu haben
			cornersV[2] = cornersV[3];
			cornersV[3] = save;

			for (int i = 0; i < cornersV.Length; i++) // Alle Ecken durchgehen
			{
				Vector2D connectionRecV;
				if (i != cornersV.Length - 1) // Die connection ist immer mit dem nächsten Punkt in der Reihe
					connectionRecV = cornersV[i + 1] - cornersV[i];
				else // Bei dem letzten punkt wieder auf den ersten springen
					connectionRecV = cornersV[0] - cornersV[i];

				//Durchgehen aller Pfadpunkte und schauen ob innerhalb des Radius ein Stück des Rechtecks ist
				foreach (var item in CurrentMap.BalloonPath)
				{
					// Nächster Punkt auf der Gerade des Rechtecks berechnet
					float closestPointDistance = -1 * (((cornersV[i].X - item.X) * connectionRecV.X + (cornersV[i].Y - item.Y) * connectionRecV.Y) / (connectionRecV.X * connectionRecV.X + connectionRecV.Y * connectionRecV.Y));
					if (closestPointDistance < 0) // Sollte der Punkt außerhalb der Länge des Rechtecks liegen wird auf die entsprechende Seite gesetzt
						closestPointDistance = 0;
					else if (closestPointDistance > 1)
						closestPointDistance = 1;

					if ((cornersV[i] + closestPointDistance * connectionRecV - item).Magnitude < 24) // Länge des Verbindungsvektors überprüfen // TODO: Mit StaticInfo verbinden
						return false;
				}

				// Alle Hitboxen des Pfades durchgehen und auf Kollisionen kontrollieren
				for (int j = 0; j < CurrentMap.BalloonPath.Length - 1; j++)
				{
					for (int k = 0; k < 2; k++) // Hitbox ober- und unterhalb kontrollieren
					{
						Vector2D pathLocationV = CurrentMap.BallonPathHitbox[k, j * 2];
						Vector2D pathDirectionV = CurrentMap.BallonPathHitbox[k, j * 2 + 1] - pathLocationV;
						// Wert der Variable für die Geradengleichung an der Schnittstelle
						float collisionVariablePathF = ((pathLocationV.Y - cornersV[i].Y) * connectionRecV.X + (cornersV[i].X - pathLocationV.X) * connectionRecV.Y) / (pathDirectionV.X * connectionRecV.Y - pathDirectionV.Y * connectionRecV.X);
						float collisionVariableRecF = ((cornersV[i].Y - pathLocationV.Y) * pathDirectionV.X + (pathLocationV.X - cornersV[i].X) * pathDirectionV.Y) / (connectionRecV.X * pathDirectionV.Y - connectionRecV.Y * pathDirectionV.X);
						// Kontrolle, ob die Schnittstelle zwischen Gerade und Seite des Rechtecks innerhalb der Intervalle von [0,1] liegt
						if (collisionVariablePathF >= 0 && collisionVariablePathF <= 1 && collisionVariableRecF >= 0 && collisionVariableRecF <= 1)
							return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Kontrolliert, ob das Rechteck mit einem Hindernis, Turm oder dem Pfad kollidiert.
		/// </summary>
		/// <param name="rect">Zu kontrollierendes Rechteck</param>
		/// <returns>Gibt True zurück, wenn keine Kollision vorliegt</returns>
		public bool IsTowerValidPosition(Rectangle rect)
		{
			foreach (var item in Towers)  //Überprüft, ob es eine Kollision mit einem Turm gibt
				if (item.Hitbox.IntersectsWith(rect)) 
					return false;

			foreach (var item in CurrentMap.Obstacles) //Überprüft, ob es eine Kollision mit einem Hindernis gibt
				if (item.Hitbox.IntersectsWith(rect))
					return false;

			return TowerCollisionPath(rect); //Überprüft, ob es eine Kollision mit dem Pfad gibt
		}
	}
}
