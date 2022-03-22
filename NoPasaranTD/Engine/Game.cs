using NoPasaranTD.Data;
using NoPasaranTD.Model;
using NoPasaranTD.Networking;
using NoPasaranTD.Utilities;
using NoPasaranTD.Visuals.Ingame;
using NoPasaranTD.Visuals.Ingame.GameOver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NoPasaranTD.Engine
{
	public class Game : IDisposable
	{
		public uint CurrentTick { get; private set; }
		public NetworkHandler NetworkHandler { get; }
		public WaveManager WaveManager { get; }

		public Map CurrentMap { get; }
		public List<Balloon>[] Balloons { get; }
		public List<Tower> Towers { get; }
		public UILayout UILayout { get; }

		public int Money { get; set; } = StaticInfo.StartMoney;
		public int HealthPoints { get; set; } = StaticInfo.StartHP;
		public bool GodMode { get; set; } = false;
		public bool Paused { get; set; } = false;
		public int Round { get; set; } = 1;

		/// <summary>
		/// Initialisiert ein neues Spiel
		/// </summary>
		/// <param name="map">Die Map die gespielt werden soll</param>
		/// <param name="networkHandler">Der genutzte Networkhandler</param>
		/// <param name="isActive">Ist das Spiel ein Aktives Spiel oder ein Hintergrundspiel</param>
		public Game(Map map, NetworkHandler networkHandler)
		{
			CurrentMap = map;
			NetworkHandler = networkHandler;
			NetworkHandler.Game = this;
			WaveManager = new WaveManager(this, 50);

			Balloons = new List<Balloon>[CurrentMap.BalloonPath.Length - 1];
			Towers = new List<Tower>();
			UILayout = new UILayout(this);

			InitNetworkHandler();
			InitBalloons();
		}

		private void InitNetworkHandler()
		{
			NetworkHandler.EventHandlers.Add("AddTower", AddTower);
			NetworkHandler.EventHandlers.Add("RemoveTower", RemoveTower);
			NetworkHandler.EventHandlers.Add("UpgradeTower", UpgradeTower);
			NetworkHandler.EventHandlers.Add("ModeChangeTower", ModeChangeTower);
			NetworkHandler.EventHandlers.Add("Accelerate", AccelerateGame);
			NetworkHandler.EventHandlers.Add("ContinueRound", StartRound);
			NetworkHandler.EventHandlers.Add("ToggleAutoStart", ToggelAutoStart);
            NetworkHandler.EventHandlers.Add("UpdateHealth", UpdateHealth);
            
        }

		private void InitBalloons()
		{
			for (int i = 0; i < Balloons.Length; i++)
				Balloons[i] = new List<Balloon>();
		}

        public void Dispose()
        {
            UILayout.Dispose();
            NetworkHandler.Dispose();
			CurrentMap.Dispose();
        }

        #region Game logic region
        public void Update()
		{
			if (Paused && NetworkHandler.OfflineMode) return; // Abfragen ob das Spiel legitim pausiert ist
			if (HealthPoints <= 0) return; // Abfragen ob das Spiel vorbei ist
			NetworkHandler.Update();

			WaveManager.Update();
			for (int i = 0; i < Balloons.Length; i++)
			{
				for (int j = Balloons[i].Count - 1; j >= 0; j--)
				{ // Aktualisiere Ballons
					Balloons[i][j].PathPosition += 0.045f * StaticInfo.GetBalloonVelocity(Balloons[i][j].Type);
					if (Balloons[i][j].PathPosition >= CurrentMap.PathLength)
					{
						if (!GodMode)
						{
							
                            NetworkHandler.InvokeEvent("UpdateHealth", (int)(HealthPoints - Balloons[i][j].Strength));
                        }

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
			// Abfrage ob das Spiel pausiert oder vorbei ist
			if (Paused || HealthPoints <= 0) return;
			UILayout.KeyUp(e);
		}

		public void KeyPress(KeyPressEventArgs e)
		{
			// Abfrage ob das Spiel pausiert oder vorbei ist
			if (Paused || HealthPoints <= 0) return;
			UILayout.KeyPress(e);
		}

		public void KeyDown(KeyEventArgs e)
		{
			if ((HealthPoints > 0 || GodMode) && e.KeyCode == Keys.Escape)
			{ // Lade Pause Menü und pausiere das Spiel im Offline Modus
			  // (Sofern Escape gedrückt wurde und der Spieler noch lebt oder sich im Gott Modus befindet)
				TogglePauseMenu();
			}

			// Abfrage ob das Spiel pausiert oder vorbei ist
			if (Paused || HealthPoints <= 0) return;
			UILayout.KeyDown(e);
		}

		public void MouseUp(MouseEventArgs e)
		{
			// Abfrage ob das Spiel pausiert oder vorbei ist
			if (Paused || HealthPoints <= 0) return;
			UILayout.MouseUp(e);
		}

		public void MouseDown(MouseEventArgs e)
		{
			// Abfrage ob das Spiel pausiert oder vorbei ist
			if (Paused || HealthPoints <= 0) return;
			UILayout.MouseDown(e);
		}

		public void MouseMove(MouseEventArgs e)
		{
			// Abfrage ob das Spiel pausiert oder vorbei ist
			if (Paused || HealthPoints <= 0) return;
			UILayout.MouseMove(e);
		}

		public void MouseWheel(MouseEventArgs e)
		{
			// Abfrage ob das Spiel pausiert oder vorbei ist
			if (Paused || HealthPoints <= 0) return;
			UILayout.MouseWheel(e);
		}
		#endregion

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
				if(!GodMode) Money += damage;
			}
			else
			{
				if(!GodMode) Money += (int)Balloons[segment][index].Value; // Nur für jede zerstörte Schicht Geld geben und nicht für theoretischen Schaden
				tower.NumberKills += (ulong)Balloons[segment][index].Strength;
				Balloons[segment].RemoveAt(index);
			}
			WaveManager.Update();
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
						else if (tower.GetBalloonFunc(Balloons[item][i], Balloons[currentSelectedBalloon.segment][currentSelectedBalloon.index]))
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
			(t as Tower).IsSelected = false;
			(t as Tower).IsPlaced = true;			

			Towers.Add((Tower)t);
			Towers[Towers.Count - 1].SearchSegments(CurrentMap);
			if (!GodMode)
				Money -= (int)StaticInfo.GetTowerPrice(t.GetType());
		}

		private void RemoveTower(object t)
		{
			Tower targetTower = FindTowerID(t);

			Tower selectedTower = UILayout.TowerDetailsContainer.Context;
			if (selectedTower != null && selectedTower.ID == targetTower.ID)
				UILayout.TowerDetailsContainer.Visible = false;
			Money += (int)targetTower.SellPrice;
			Towers.Remove(targetTower);
		}

        public void UpdateHealth(object h)
        {
            HealthPoints = (int)h;
            if (HealthPoints <= 0) // Fragt ab ob der Spieler gerade verloren hat
                Program.LoadScreen(new GuiGameOver()); // Lade GuiGameOver falls dies der Fall ist
        }

		public void UpgradeTower(object t)
        {
			Tower tower = FindTowerID(t);
			if (!GodMode)
				Money -= (int)tower.UpgradePrice;
			tower.Level++;
			tower.SearchSegments(CurrentMap);
		}

		public void ModeChangeTower(object t)
        {
			Tower targetTower = FindTowerID(t);
			targetTower.TargetMode = (t as Tower).TargetMode;

			Tower selectedTower = UILayout.TowerDetailsContainer.Context;
			if (selectedTower != null && selectedTower.ID == targetTower.ID)
				UILayout.TowerDetailsContainer.Context = targetTower;
		}

		public void StartRound(object t)
        {
			WaveManager.StartSpawn();
        }

		public void AccelerateGame(object t)
        {
			if (StaticEngine.TickAcceleration == 8)
				StaticEngine.TickAcceleration = 1;
			else
				StaticEngine.TickAcceleration *= 2;
		}

		public void TogglePauseMenu()
        {
			Program.LoadScreen((Paused = !Paused) ?
					new GuiPauseMenu(this) : null);
		}

		public void ToggelAutoStart(object t)
        {
			WaveManager.AutoStart = !WaveManager.AutoStart;
        }

		private Tower FindTowerID(object t)
        {
			foreach(Tower tower in Towers)
            {
				if (tower.ID == (t as Tower).ID)
					return tower;
            }

			throw new Exception("Tower not found");
		}

    }
}

