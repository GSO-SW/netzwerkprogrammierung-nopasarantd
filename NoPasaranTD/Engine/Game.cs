using NoPasaranTD.Data;
using NoPasaranTD.Model;
using NoPasaranTD.Utilities;
using NoPasaranTD.Visuals.Ingame;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NoPasaranTD.Engine
{
    public class Game
	{
		private readonly Random random = new Random();

		public uint CurrentTick { get; private set; }

		public Map CurrentMap { get; }
		public List<Balloon> Balloons { get; }
		public List<Tower> Towers { get; }
		public UILayout UILayout { get; }

		public int Money { get; set; }
		public int HealthPoints { get; set; }

		public Game(Map map)
		{
			CurrentMap = map;
			Towers = new List<Tower>();
			Balloons = new List<Balloon>();
			UILayout = new UILayout(this);
			Money = StaticInfo.StartMoney;
			HealthPoints = StaticInfo.StartHP;
		}

        #region Game logic region
        public void Update()
		{
			// Aktualisiere Türme
			for (int i = Towers.Count - 1; i >= 0; i--)
				Towers[i].Update(this);

            for (int i = Balloons.Count - 1; i >= 0; i--)
			{ // Aktualisiere Ballons
				Balloons[i].PathPosition += 0.075f * StaticInfo.GetBalloonVelocity(Balloons[i].Type);
				if (Balloons[i].PathPosition >= CurrentMap.PathLength)
				{
					HealthPoints -= (int)Balloons[i].Strength;
					Balloons.RemoveAt(i);
				}
			}
            UILayout.Update();

            ManageBalloonSpawn(); // Spawne Ballons
			CurrentTick++;
		}

		public void Render(Graphics g)
		{
			{ // Zeichne Karte
				float scaledWidth = (float)StaticEngine.RenderWidth / CurrentMap.BackgroundImage.Width;
				float scaledHeight = (float)StaticEngine.RenderHeight / CurrentMap.BackgroundImage.Height;

				Matrix m = g.Transform;
				g.ScaleTransform(scaledWidth, scaledHeight);
				g.DrawImageUnscaled(CurrentMap.BackgroundImage, 0, 0);
				g.Transform = m;
            }

            for (int i = Balloons.Count - 1; i >= 0; i--)
			{ // Zeichne Ballons
				Brush brush;
				switch (Balloons[i].Type)
				{ // TODO: Ändern durch Texturen
					case BalloonType.Red: brush = Brushes.Red; break;
					case BalloonType.Blue: brush = Brushes.Blue; break;
					case BalloonType.Green: brush = Brushes.Green; break;
					case BalloonType.Purple: brush = Brushes.Purple; break;
					case BalloonType.Black: brush = Brushes.Black; break;
					case BalloonType.Gold: brush = Brushes.Gold; break;
					default: continue; // Ignoriere jeden unbekannten Ballon
				}

				Vector2D pos = CurrentMap.GetPathPosition(
					StaticEngine.RenderWidth, 
					StaticEngine.RenderHeight,
					Balloons[i].PathPosition
				);

				g.FillEllipse(brush, pos.X - 5, pos.Y - 6, 10, 12);
            }

			for (int i = Towers.Count - 1; i >= 0; i--)
				Towers[i].Render(g);
            UILayout.Render(g);
		}

		public void KeyUp(KeyEventArgs e) => UILayout.KeyUp(e);
		public void KeyDown(KeyEventArgs e) => UILayout.KeyDown(e);

		public void MouseUp(MouseEventArgs e) => UILayout.MouseUp(e);
		public void MouseDown(MouseEventArgs e) => UILayout.MouseDown(e);
		public void MouseMove(MouseEventArgs e) => UILayout.MouseMove(e);
		public void MouseWheel(MouseEventArgs e) => UILayout.MouseWheel(e);
		#endregion

		private void ManageBalloonSpawn()
		{
			if (CurrentTick % 1200 == 0)
			{ // Spawne jede Sekunde einen Ballon
				Balloon balloon = new Balloon
				{
					PathPosition = 0
				};

				BalloonType[] values = (BalloonType[])Enum.GetValues(typeof(BalloonType));
				balloon.Type = values[random.Next(1, values.Length)];
				Balloons.Add(balloon);
			}
		}

        /// <summary>
        /// Gibt einen Ballon in Reichweite des Turms zurück der am weitesten ist
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Index des Ziels in der Liste Balloons.</br>
        /// Ohne Ballon in Reichweite -1</returns>
        public int FindTargetForTower(Tower tower)
        {
            List<int> balloonsInRange = new List<int>();
			// Alle Ballons in der Reichweite des Turms bestimmen
			for (int i = Balloons.Count - 1; i >= 0; i--)
            {
                Vector2D currentPosition = CurrentMap.GetPathPosition(Balloons[i].PathPosition); // Position des Ballons
                Vector2D towerCentre = new Vector2D(tower.Hitbox.Location.X + tower.Hitbox.Width / 2, tower.Hitbox.Location.Y + tower.Hitbox.Height / 2); // Zentrale Position des Turmes
                if ((currentPosition - towerCentre).Magnitude <= tower.Range) //Länge des Verbindungsvektors zwischen Turmmitte und dem Ballon muss kleiner sein als der Radius des Turmes
                {
                    balloonsInRange.Add(i); // Sammeln aller Ballons in der Reichweite, nur notwendig sollte eine Kontrolle für Obstacle Collison eingefügt werden
                    // Checken ob der neue Ballon weiter ist als der bisher weiteste
                    if (tower.GetBalloonFunc(Balloons[balloonsInRange.Count - 1], Balloons[balloonsInRange[0]]))
                    {
						balloonsInRange.Insert(0, i); // Bisherige Wahl an Stelle 0 schreiben
						balloonsInRange.RemoveAt(balloonsInRange.Count - 1);
                    }
				}                   
            }
            if (balloonsInRange.Count == 0) // Sollte kein Ballon in der Reichweite sein
                return -1;

            return balloonsInRange[0];
        }

        /// <summary>
        /// Kontrolliert, ob das Rechteck mit einem Hindernis, Turm oder dem Pfad kollidiert.
        /// </summary>
        /// <param name="rect">Zu kontrollierendes Rechteck</param>
        /// <returns>Gibt True zurück, wenn keine Kollision vorliegt</returns>
        public bool IsTowerValidPosition(Rectangle rect)
		{
			for (int i = Towers.Count - 1; i >= 0; i--)  //Überprüft, ob es eine Kollision mit einem Turm gibt
				if (Towers[i].Hitbox.IntersectsWith(rect))
					return false;

			for (int i = CurrentMap.Obstacles.Count - 1; i >= 0; i--) //Überprüft, ob es eine Kollision mit einem Hindernis gibt
				if (CurrentMap.Obstacles[i].Hitbox.IntersectsWith(rect))
					return false;

			return CurrentMap.IsCollidingWithPath(
				StaticEngine.RenderWidth, 
				StaticEngine.RenderHeight, 
				rect
			); //Überprüft, ob es eine Kollision mit dem Pfad gibt
		}

		/// <summary>
		/// Fügt einen Ballon eine bestimmte Menge an Schaden hinzu.<br/>
		/// Sollte der Ballon danach keinen gültigen Typ mehr haben, wird er von der Liste entfernt.
		/// </summary>
		/// <param name="index">Der index des Ballons</param>
		/// <param name="damage">Die Anzahl an Lebenspunkten die entfernt werden sollen</param>
		public void DamageBalloon(int index, int damage, Tower tower)
		{
			if (Balloons[index].Type - damage > BalloonType.None)
			{
				Balloons[index].Type -= damage; // Aufaddieren des Geldes
				Towers[indexTower].NumberKills += (ulong)damage;
				Money += damage;
			}
			else
			{
				Money += (int)Balloons[index].Strength; // Nut für jede zerstörte Schicht Geld geben und nicht für theoretischen Schaden
				tower.NumberKills += (ulong)Balloons[index].Strength;
				Balloons.RemoveAt(index);
			}
		}

		public void AddTower(Tower t)
		{
			// TODO network communication
			t.IsSelected = false;
			Towers.Add(t);
		}

		public void RemoveTower(Tower t)
		{
			// TODO network communication
			Towers.Remove(t);
		}
	}
}
