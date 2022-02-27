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
		/// Kontrolliert, ob das angegebene Recheck einen Abstand von X Einheiten zum nächsten Pfadpunkt hat und ob keine Hitbox des Pfades getroffen wurde
		/// </summary>
		/// <param name="rect">Zu überprüfendes Rechteck</param>
		/// <returns>False wenn es eine Überschneidung gibt</returns>
		public bool TowerCollisionPathCorner(Rectangle rect)
        {
            Vector2D[] cornersV = new Vector2D[3]; // Speichern der Ecken des Rechtecks
            for (int i = 0; i < 2; i++) // Alle Ecken durchgehen
                for (int j = 0; j < 2; j++)
                    cornersV[i * 2 + j] = new Vector2D(rect.X + i * rect.Width, rect.Y + j * rect.Height); // Ecken abspeichern
            Vector2D save = cornersV[2]; // Ecke 2 mit 3 tauschen um eine durchgehende Reihenfolge zu haben
            cornersV[2] = cornersV[3];
            cornersV[3] = save;

            for (int i = 0; i < cornersV.Length; i++) // Alle Ecken durchgehen
            {
                Vector2D connection;
                if (i != cornersV.Length - 1) // Die connection ist immer mit dem nächsten Punkt in der Reihe
                    connection = cornersV[i] - cornersV[i + 1];
                else // Bei dem letzten punkt wieder auf den ersten springen
                    connection = cornersV[i] - cornersV[0];

                // Durchgehen aller Pfadpunkte und schauen ob innerhalb des Radius ein Stück des Rechtecks ist
                foreach (var item in CurrentMap.BalloonPath)
                {
                    // Nächster Punkt auf der Gerade des Rechtecks berechnet
                    float closestPointDistance = -1 * (((cornersV[i].X - item.X) * connection.X + (cornersV[i].Y - item.Y) * connection.Y) / (connection.X * connection.X + connection.Y + connection.Y));
                    if (closestPointDistance < 0) // Sollte der Punkt außerhalb der Länge des Rechtecks liegen wird auf die entsprechende Seite gesetzt
                        closestPointDistance = 0;
                    else if (closestPointDistance > 1)
                        closestPointDistance = 1;

                    if ((closestPointDistance * connection).Magnitude < 100) // Länge des Verbindungsvektors überprüfen // TODO: Mit StaticInfo verbinden
                        return false;
                }

                // Alle Hitboxen des Pfades durchgehen und auf Kollisionen kontrollieren
                for (int j = 0; j < CurrentMap.BallonPathHitbox.GetLength(1) - 1; j++)
                {
                    for (int k = 0; k < 2; k++) // Hitbox unter und oberhalb des Pfades checken
                    {
                        Vector2D currentV = CurrentMap.BallonPathHitbox[k, j]; // Ortsvektor des Hitboxteils
                        Vector2D pathV = CurrentMap.BallonPathHitbox[k, j + 1] - currentV; // Richtungsvektor des Hitbox Teils
                        // Kontrolle, ob die Rechteckseite und der Pfad kollinear sind
                        float collinearF = connection.X * pathV.Y - connection.Y * pathV.X;
                        if (collinearF != 0) // Nicht kollinear
                        {
                            // Schnittpunkt Wert bestimmen
                            float collisionF = ((currentV.Y - cornersV[i].Y) * connection.X + (currentV.X - cornersV[i].X) * connection.Y) / collinearF;
                            if (collisionF < 1 && collisionF > 0) // Kontrolle, ob der Schnittpunkt im Intervall der Gerade liegt
                                return false;
                        }
                        else // kollinear
                        {
                            float checkF = (currentV.X - cornersV[i].X) / pathV.X; // Kontrolle, ob die Geraden der Hitbox und des Rechtecks gleich sind
                            if (checkF == (currentV.Y - cornersV[i].Y) / pathV.Y && checkF > 0 && checkF < 1) // Gleiche Gerade und innerhalb des Intervals
                                return false;
                        }
                    }
                }
            }
            return true;
        }
	}
}
