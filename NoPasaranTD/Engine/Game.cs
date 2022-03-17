﻿using NoPasaranTD.Data;
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
		private static readonly Random RANDOM = new Random();

		public uint CurrentTick { get; private set; }
		public List<Balloon>[] Balloons { get; }
		public List<Tower> Towers { get; }

		public int Money { get; set; }
		public int HealthPoints { get; set; }
		public bool Paused { get; set; }

		public Map CurrentMap { get; }
		public NetworkHandler NetworkHandler { get; }
		public UILayout UILayout { get; }
		public Game(Map map, NetworkHandler networkHandler)
		{
			CurrentMap = map;
			NetworkHandler = networkHandler;
			UILayout = new UILayout(this);

			CurrentTick = 0;
			Balloons = new List<Balloon>[CurrentMap.BalloonPath.Length - 1];
			Towers = new List<Tower>();

			Money = StaticInfo.StartMoney;
			HealthPoints = StaticInfo.StartHP;
			Paused = false;

			InitNetworkHandler();
			InitBalloons();
		}

		private void InitNetworkHandler()
		{
			NetworkHandler.EventHandlers.Add("AddTower", AddTower);
			NetworkHandler.EventHandlers.Add("RemoveTower", RemoveTower);
		}

		private void InitBalloons()
		{
			for (int i = 0; i < Balloons.Length; i++)
				Balloons[i] = new List<Balloon>();
		}

		#region Game logic region
		public void Update()
		{
			if (Paused && NetworkHandler.OfflineMode) return;
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

					g.FillEllipse(brush, 
						pos.X - StaticInfo.BalloonSize.Width / 2, 
						pos.Y - StaticInfo.BalloonSize.Height / 2, 
						StaticInfo.BalloonSize.Width,
						StaticInfo.BalloonSize.Height
					);
				}
			}

			foreach (var item in CurrentMap.Obstacles)
			{ // Hindernisse rendern
				g.FillRectangle(Brushes.Red, 
					(float)item.Hitbox.X / CurrentMap.Dimension.Width * StaticEngine.RenderWidth,
					(float)item.Hitbox.Y / CurrentMap.Dimension.Height * StaticEngine.RenderHeight,
					(float)item.Hitbox.Width / CurrentMap.Dimension.Width * StaticEngine.RenderWidth,
					(float)item.Hitbox.Height / CurrentMap.Dimension.Height * StaticEngine.RenderHeight
				);
            }

			for (int i = Towers.Count - 1; i >= 0; i--)
				Towers[i].Render(g);
			UILayout.Render(g);
		}

		public void KeyUp(KeyEventArgs e)
		{
			if (Paused) return;
			UILayout.KeyUp(e);
		}

		public void KeyPress(KeyPressEventArgs e)
		{
			if (Paused) return;
			UILayout.KeyPress(e);
		}

		public void KeyDown(KeyEventArgs e)
		{
			if (HealthPoints > 0 && e.KeyCode == Keys.Escape)
			{
				Program.LoadScreen((Paused = !Paused) ?
					new GuiPauseMenu(this) : null);
			}

			if (Paused) return;
			UILayout.KeyDown(e);
		}

		public void MouseUp(MouseEventArgs e)
		{
			if (Paused) return;
			UILayout.MouseUp(e);
		}

		public void MouseDown(MouseEventArgs e)
		{
			if (Paused) return;
			UILayout.MouseDown(e);
		}

		public void MouseMove(MouseEventArgs e)
		{
			if (Paused) return;
			UILayout.MouseMove(e);
		}

		public void MouseWheel(MouseEventArgs e)
		{
			if (Paused) return;
			UILayout.MouseWheel(e);
		}
		#endregion

		private void ManageBalloonSpawn()
		{
			if (CurrentTick % 1000 == 0)
			{ // Spawne jede Sekunde einen Ballon
				BalloonType[] values = (BalloonType[])Enum.GetValues(typeof(BalloonType));
				Balloons[0].Add(new Balloon(values[RANDOM.Next(1, values.Length - 1)]));
			}
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
				if (CurrentMap.Obstacles[i].Hitbox.IntersectsWith(CurrentMap.GetScaledRect(StaticEngine.RenderWidth, StaticEngine.RenderHeight, rect)))
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
				Money += (int)Balloons[segment][index].Value; // Nur für jede zerstörte Schicht Geld geben und nicht für theoretischen Schaden
				tower.NumberKills += (ulong)Balloons[segment][index].Strength;
				Balloons[segment].RemoveAt(index);
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
						if (!foundBalloon && CheckBalloonIfHiddenPos(Balloons[item][i].PathPosition, tower)) // Der erste Ballon in der Reichweite wird nicht gecheckt ob er weiter ist als er selbst
						{
							currentSelectedBalloon = (item, i);
							foundBalloon = true;
						}
						// Checken, ob der neue Ballon weiter ist als der bisher weiteste
						else if (tower.GetBalloonFunc(Balloons[item][i], Balloons[currentSelectedBalloon.Item1][currentSelectedBalloon.Item2]))
							if (CheckBalloonIfHiddenPos(Balloons[item][i].PathPosition, tower))
								currentSelectedBalloon = (item, i);
					}
				}
			}
			if (!foundBalloon) // Sollte kein Ballon in der Reichweite gefunden worden sein
				return (-1, -1);

			return currentSelectedBalloon;
		}

		/// <summary>
		/// Kontrolliert, ob der Angegebene Punkt innerhalb eines nicht sichtbaren Bereiches ist
		/// </summary>
		/// <param name="balloonPos"></param>
		/// <param name="tower"></param>
		/// <returns>Gibt true zurück wenn der Ballon gesehen werden kann</returns>
		private bool CheckBalloonIfHiddenPos(float balloonPos, Tower tower)
		{
			foreach (var item in tower.NotVisibleSpots) // Alle nicht sichtbaren Stellen kontrollieren
				if (item.X < balloonPos && item.Y > balloonPos) // Sollte der Ballon innerhalb einer der nicht sichtbaren Stellen sein wird abgebrochen
					return false;
			return true;
		}

		private void AddTower(object t)
		{
			// TODO network communication
			(t as Tower).IsSelected = false;
			(t as Tower).IsPlaced = true;			

			Towers.Add((Tower)t);
			Towers[Towers.Count - 1].SearchSegments(CurrentMap);
		}

		private void RemoveTower(object t)
		{
			// TODO network communication
			Towers.Remove((Tower)t);
		}

		public void UpgradeTower(Tower t)
        {
			t.Level++;
			t.SearchSegments(CurrentMap);
        }
	}
}

