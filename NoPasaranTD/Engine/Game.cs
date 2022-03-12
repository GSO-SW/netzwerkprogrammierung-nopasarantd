using NoPasaranTD.Data;
using NoPasaranTD.Model;
using NoPasaranTD.Networking;
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

		public NetworkHandler NetworkHandler { get; }
		public Map CurrentMap { get; }
		public List<Balloon>[] Balloons { get; private set; }
		public List<Tower> Towers { get; }
		public UILayout UILayout { get; }

		public int Money { get; set; }
		public int HealthPoints { get; set; }

		public Game(Map map, NetworkHandler networkHandler)
		{
			NetworkHandler = networkHandler;
			CurrentMap = map;
			Balloons = new List<Balloon>[CurrentMap.BalloonPath.Length - 1];
			InitBalloon();
			Towers = new List<Tower>();
			UILayout = new UILayout(this);
			Money = StaticInfo.StartMoney;
			HealthPoints = StaticInfo.StartHP;
			InitNetworkHandler();
		}

		private void InitNetworkHandler()
		{
			NetworkHandler.EventHandlers.Add("AddTower", AddTower);
			NetworkHandler.EventHandlers.Add("RemoveTower", RemoveTower);
		}

		#region Game logic region
		public void Update()
		{
			for (int i = 0; i < Balloons.Length; i++)
			{
				for (int j = Balloons[i].Count - 1; j >= 0; j--)
				{ // Aktualisiere Ballons
					Balloons[i][j].PathPosition += 0.045f * StaticInfo.GetBalloonVelocity(Balloons[i][j].Type);
					if (Balloons[i][j].PathPosition >= CurrentMap.PathLength)
					{
						HealthPoints -= (int)Balloons[i][j].Strength;
						Balloons[i].RemoveAt(j);
					}
					else if (CurrentMap.CheckBalloonPosFragment(Balloons[i][j].PathPosition, (uint)i))
					{
						Balloons[i + 1].Add(Balloons[i][j]); // Einfügen des Ballons in das nächste Pfadsegment
						Balloons[i].RemoveAt(j); // Entfernen des Ballons aus dem letzten Pfadsegment
					}
				}
			}

			// Aktualisiere Türme
			for (int i = Towers.Count - 1; i >= 0; i--)
				Towers[i].Update(this);

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

			foreach (var item in Balloons)
			{
				for (int i = 0; i < item.Count; i++)
				{
					Brush brush;
					switch (item[i].Type)
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
						item[i].PathPosition
					);

					g.FillEllipse(brush, pos.X - 5, pos.Y - 6, 10, 12);
				}

				for (int i = Towers.Count - 1; i >= 0; i--)
					Towers[i].Render(g);
				UILayout.Render(g);
			}
		}

		public void KeyUp(KeyEventArgs e) => UILayout.KeyUp(e);
		public void KeyDown(KeyEventArgs e) => UILayout.KeyDown(e);

		public void MouseUp(MouseEventArgs e) => UILayout.MouseUp(e);
		public void MouseDown(MouseEventArgs e) => UILayout.MouseDown(e);
		public void MouseMove(MouseEventArgs e) => UILayout.MouseMove(e);
		public void MouseWheel(MouseEventArgs e) => UILayout.MouseWheel(e);
		#endregion

		private void InitBalloon()
		{
			for (int i = 0; i < Balloons.Length; i++)
				Balloons[i] = new List<Balloon>();
		}

		private void ManageBalloonSpawn()
		{
			if (CurrentTick % 1200 == 0)
			{ // Spawne jede Sekunde einen Ballon
				Balloon balloon = new Balloon
				{
					PathPosition = 0
				};

				BalloonType[] values = (BalloonType[])Enum.GetValues(typeof(BalloonType));
				balloon.Type = values[random.Next(1, values.Length - 1)];
				Balloons[0].Add(balloon);
			}
		}

		/// <summary>
		/// Gibt einen Ballon in Reichweite des Turms zurück, der das gesuchte Kriterium, des Turmes, am besten erfüllt
		/// </summary>
		/// <param name="index"></param>
		/// <returns>Tuple mit item1 = Pfadabschnitt, item2 = Index im Pfadabschnitt</br>
		/// Ohne Ballon in Reichweite -1</returns>
		public (int segment, int index) FindTargetForTower(Tower tower)
		{
			(int segment, int index) currentSelectedBalloon = (0, 0); // Abspeichern der derzeit weitesten bekannten Position eines Ballons 
			bool foundBalloon = false; // Variable zum festhalten, ob es einen Ballon in der Reichweite gibt
			Vector2D towerCentre = new Vector2D(tower.Hitbox.Location.X + tower.Hitbox.Width / 2, tower.Hitbox.Location.Y + tower.Hitbox.Height / 2); // Zentrale Position des Turmes

			foreach (var item in tower.SegmentsInRange) // Alle Segmente in Reichweite des Turmes durchgehen
			{
				for (int i = Balloons[item].Count - 1; i >= 0; i--)
				{
					Vector2D currentPosition = CurrentMap.GetPathPosition(StaticEngine.RenderWidth, StaticEngine.RenderHeight, Balloons[item][i].PathPosition); // Position des Ballons
					if ((currentPosition - towerCentre).Magnitude <= tower.Range) //Länge des Verbindungsvektors zwischen Turmmitte und dem Ballon muss kleiner sein als der Radius des Turmes
					{
						if (!foundBalloon) // Der erste Ballon in der Reichweite wird nicht gecheckt ob er weiter ist als er selbst
						{
							currentSelectedBalloon = (item, i);
							foundBalloon = true;
						}
						// Checken ob der neue Ballon weiter ist als der bisher weiteste
						else if (tower.GetBalloonFunc(Balloons[item][i], Balloons[currentSelectedBalloon.Item1][currentSelectedBalloon.Item2]))
							currentSelectedBalloon = (item, i);
					}
				}
			}
			if (!foundBalloon) // Sollte kein Ballon in der Reichweite gefunden worden sein
				return (-1, -1);

			return currentSelectedBalloon;
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
		/// <param name="index">Der Index des Ballons</param>
		/// <param name="damage">Die Anzahl an Lebenspunkten die entfernt werden sollen</param>
		public void DamageBalloon(int segment, int index, int damage, Tower tower)
		{
			if (Balloons[segment][index].Type - damage > BalloonType.None)
			{
				Balloons[segment][index].Type -= damage; // Aufaddieren des Geldes
				Money += damage;
			}
			else
			{
				Money += (int)Balloons[segment][index].Strength; // Nur für jede zerstörte Schicht Geld geben und nicht für theoretischen Schaden
				tower.NumberKills += (ulong)Balloons[segment][index].Strength;
				Balloons[segment].RemoveAt(index);
			}
		}

		private void AddTower(object t)
		{
			// TODO network communication
			(t as Tower).IsSelected = false;
			(t as Tower).IsPlaced = true;			

			Towers.Add((Tower)t);
			Towers[Towers.Count - 1].FindSegmentsInRange(CurrentMap);
		}

		private void RemoveTower(object t)
		{
			// TODO network communication
			Towers.Remove((Tower)t);
		}
	}
	}

