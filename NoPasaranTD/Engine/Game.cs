﻿using NoPasaranTD.Data;
using NoPasaranTD.Model;
using NoPasaranTD.Networking;
using NoPasaranTD.Utilities;
using NoPasaranTD.Visuals;
using NoPasaranTD.Visuals.Ingame;
using NoPasaranTD.Visuals.Ingame.GameOver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Engine
{
    public class Game : IDisposable
    {
        public uint CurrentTick { get; set; }
        public NetworkHandler NetworkHandler { get; }
        public WaveManager WaveManager { get; }

        public Map CurrentMap { get; }
        public List<Balloon>[] Balloons { get; }
        public List<Tower> Towers { get; }
        public List<Tower> VTowers { get; } // Speichert die Türme die noch nicht aktiv sind und nur angezeigt werden
        public UILayout UILayout { get; }

        public int Money { get; set; } = StaticInfo.StartMoney;
        public int HealthPoints { get; set; } = StaticInfo.StartHP;
        public bool GodMode { get; set; } = false;
        public bool Paused { get; set; } = false;
        public int Round { get; set; } = 1;

        public NotifyCollection<string> Messages { get; set; } = new NotifyCollection<string>();

		private StaticDisplay StaticDisplay { get; } = null;

        // Mouse Cursor Packeteinstellungen
		private int MouseSendInterval = 200;
        private static List<(int X, int Y, int TTL, int currentTick)> usersMousePos = new List<(int X, int Y, int TTL, int currentTick)>();           
        private static List<string> usersMouseTag = new List<string>();

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
            VTowers = new List<Tower>();
            UILayout = new UILayout(this);

            InitNetworkHandler();
            InitBalloons();
        }

        private void InitNetworkHandler()
        {
            NetworkHandler.EventHandlers.Add("AddTower", AddVTower);
            NetworkHandler.EventHandlers.Add("RemoveTower", RemoveTower);
            NetworkHandler.EventHandlers.Add("UpgradeTower", UpgradeTower);
            NetworkHandler.EventHandlers.Add("ModeChangeTower", ModeChangeTower);
            NetworkHandler.EventHandlers.Add("Accelerate", AccelerateGame);
            NetworkHandler.EventHandlers.Add("ContinueRound", StartRound);
            NetworkHandler.EventHandlers.Add("ToggleAutoStart", ToggelAutoStart);
            NetworkHandler.EventHandlers.Add("AddBalloon", AddBalloon);
            NetworkHandler.EventHandlers.Add("SendMessage", SendMessage);
            NetworkHandler.EventHandlers.Add("UpdateHealth", UpdateHealth);
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
			NetworkHandler.EventHandlers.Add("TransferMousePosition", TransferMousePosition);
		}

        /// <summary>
        /// Initialisiert die Eigenschaft der Ballons und löscht damit vorhandene Werte
        /// </summary>
        public void InitBalloons()
        {
            for (int i = 0; i < Balloons.Length; i++)
            {
                Balloons[i] = new List<Balloon>();
            }
        }

        public void Dispose()
        {
            StaticEngine.TickAcceleration = 1;

            UILayout.Dispose();
            NetworkHandler.Dispose();
            CurrentMap.Dispose();
        }

        #region Game logic region

        //private bool check = true; // TESTCODE
        //private bool check2 = true; // TESTCODE
        public void Update()
        {
            if (NetworkHandler.ResyncDelay > 0)
            {
                NetworkHandler.ResyncDelay--;
                return;
            }
            if ((Paused && NetworkHandler.OfflineMode) || NetworkHandler.Resyncing)
            {
                return; // Abfragen ob das Spiel legitim pausiert ist
            }

            if (HealthPoints <= 0)
            {
                return; // Abfragen ob das Spiel vorbei ist
            }
            //if (Round == 3 && check && NetworkHandler.IsHost) TESTCODE
            //         {
            //	NetworkHandler.ReliableUPD.SendReliableUDP("ResyncReq", 0);
            //	check = false;
            //         }
            //if (Round == 4 && check2 && NetworkHandler.IsHost)
            //{
            //	NetworkHandler.ReliableUPD.SendReliableUDP("ResyncReq", 0);
            //	check2 = false;
            //}
            NetworkHandler.Update();

            WaveManager.Update();
            for (int i = 0; i < Balloons.Length; i++)
            {
                for (int j = Balloons[i].Count - 1; j >= 0; j--)
                { // Aktualisiere Ballons
                    Balloons[i][j].PathPosition += 0.045f * StaticInfo.GetBalloonVelocity(Balloons[i][j].Type);
                    if (Balloons[i][j].PathPosition >= CurrentMap.PathLength)
                    {
                        if (!GodMode && NetworkHandler.IsHost)
                        {
                            NetworkHandler.InvokeEvent("UpdateHealth", (int)(HealthPoints - Balloons[i][j].Strength));
                        }
                        Balloons[i].RemoveAt(j);
                    }
                    else if (CurrentMap.CheckBalloonPosFragment(Balloons[i][j].PathPosition, (uint)i))
                    {
                        Balloons[i][j].CurrentSegment++; // Erhöht das Pfadsegment des Ballons
                        Balloons[i + 1].Add(Balloons[i][j]); // Einfügen des Ballons in das nächste Pfadsegment
                        Balloons[i].RemoveAt(j); // Entfernen des Ballons aus dem letzten Pfadsegment
                    }
                }
            }

            CheckVTower();
            for (int i = Towers.Count - 1; i >= 0; i--) // Alle Türme die aktiv sind updaten

				Towers[i].Update(this);

            // eigene Maus schicken
			if (CurrentTick % MouseSendInterval == 0)
            {
				if (NetworkHandler.LocalPlayer == null) return;

				var networkPackage = new NetworkPackageMousePosition();
				networkPackage.Pos = (StaticEngine.MouseX, StaticEngine.MouseY);
				networkPackage.currentTick = (int)CurrentTick;

				// TODO ergänzen: den Username mitschicken statt das id ding -26.3.2022 
				networkPackage.Username = NetworkHandler.LocalPlayer.Name;  
				
				NetworkHandler.InvokeEvent("TransferMousePosition", networkPackage);

				if (CurrentTick % 1000 == 0)
					for (int i = 0; i < usersMousePos.Count; i++)
						if (usersMousePos[i].TTL < Environment.TickCount)
						{
							usersMousePos.RemoveAt(i);
							usersMouseTag.RemoveAt(i);
						}
			}


			UILayout.Update();
			CurrentTick++;
		}

        /// <summary>
        /// Kontrolliert alle nicht aktiven Türme, ob diese aktiviert werden müssen in dem Tick
        /// </summary>
        public void CheckVTower()
        {
            for (int i = VTowers.Count - 1; i >= 0; i--)
            {
                if (VTowers[i].ActivateAtTick <= CurrentTick) // Checken, ob der Turm schon aktiv ist
                {
                    AddTower(VTowers[i]); // Turm neu abspeichern bei den aktiven Türmen
                    VTowers.RemoveAt(i);
                }
            }
        }

        public void Render(Graphics g)
        {
            // Zeichne Karte
            g.DrawImage(CurrentMap.BackgroundImage,
                0, 0, StaticEngine.RenderWidth, StaticEngine.RenderHeight);

            foreach (List<Balloon> item in Balloons)
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

            foreach (Obstacle item in CurrentMap.Obstacles)
            { // Hindernisse rendern
                g.DrawImage(item.Image,
                    (float)item.Hitbox.X / CurrentMap.Dimension.Width * StaticEngine.RenderWidth,
                    (float)item.Hitbox.Y / CurrentMap.Dimension.Height * StaticEngine.RenderHeight,
                    (float)item.Hitbox.Width / CurrentMap.Dimension.Width * StaticEngine.RenderWidth,
                    (float)item.Hitbox.Height / CurrentMap.Dimension.Height * StaticEngine.RenderHeight
                );
            }
            //Font fontArial = new Font("Arial", 10, FontStyle.Regular);
            //g.DrawString(CurrentTick + "", fontArial,new SolidBrush(Color.Black), 0, 200);

            for (int i = Towers.Count - 1; i >= 0; i--)
            {
                Towers[i].Render(g);
            }
            UILayout.Render(g);

            for (int i = VTowers.Count - 1; i >= 0; i--)
            {
                VTowers[i].Render(g);
            }

            UILayout.Render(g);
        }

			// zeichne die Maus Positionen von anderen wenn online
			if (!NetworkHandler.OfflineMode)
				for (int i = 0; i < usersMousePos.Count; i++)
                {
					g.DrawString(usersMouseTag[i], SystemFonts.DefaultFont, Brushes.Black,
						usersMousePos[i].X + 15, usersMousePos[i].Y - 5);
					g.DrawRectangle(Pens.Red, usersMousePos[i].X - 5, usersMousePos[i].Y - 5, 10, 10);
				}
					
            


		}

        public void KeyUp(KeyEventArgs e)
        {
            // Abfrage ob das Spiel pausiert oder vorbei ist
            if (Paused || HealthPoints <= 0)
            {
                return;
            }

            UILayout.KeyUp(e);
        }

        public void KeyPress(KeyPressEventArgs e)
        {
            // Abfrage ob das Spiel pausiert oder vorbei ist
            if (Paused || HealthPoints <= 0)
            {
                return;
            }

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
            if (Paused || HealthPoints <= 0)
            {
                return;
            }

            UILayout.KeyDown(e);
        }

        public void MouseUp(MouseEventArgs e)
        {
            // Abfrage ob das Spiel pausiert oder vorbei ist
            if (Paused || HealthPoints <= 0)
            {
                return;
            }

            UILayout.MouseUp(e);
        }

        public void MouseDown(MouseEventArgs e)
        {
            // Abfrage ob das Spiel pausiert oder vorbei ist
            if (Paused || HealthPoints <= 0)
            {
                return;
            }

            UILayout.MouseDown(e);
        }

        public void MouseMove(MouseEventArgs e)
        {
            // Abfrage ob das Spiel pausiert oder vorbei ist
            if (Paused || HealthPoints <= 0)
            {
                return;
            }

            UILayout.MouseMove(e);
        }

        public void MouseWheel(MouseEventArgs e)
        {
            // Abfrage ob das Spiel pausiert oder vorbei ist
            if (Paused || HealthPoints <= 0)
            {
                return;
            }

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
            {
                if (Towers[i].Hitbox.IntersectsWith(rect))
                {
                    return false;
                }
            }

            for (int i = VTowers.Count - 1; i >= 0; i--)  //Überprüft, ob es eine Kollision mit einem noch nicht platzierten Turm gibt
            {
                if (VTowers[i].Hitbox.IntersectsWith(rect))
                {
                    return false;
                }
            }

            for (int i = CurrentMap.Obstacles.Count - 1; i >= 0; i--) //Überprüft, ob es eine Kollision mit einem Hindernis gibt
            {
                if (CurrentMap.Obstacles[i].Hitbox.IntersectsWith(CurrentMap.GetScaledRect(StaticEngine.RenderWidth, StaticEngine.RenderHeight, rect)))
                {
                    return false;
                }
            }

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
                if (!GodMode)
                {
                    Money += damage;
                }
            }
            else
            {
                if (!GodMode)
                {
                    Money += (int)Balloons[segment][index].Value; // Nur für jede zerstörte Schicht Geld geben und nicht für theoretischen Schaden
                }

                tower.NumberKills += Balloons[segment][index].Strength;
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

            foreach (int item in tower.SegmentsInRange) // Alle Segmente in Reichweite des Turmes durchgehen
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
                        else if (foundBalloon && tower.GetBalloonFunc(Balloons[item][i], Balloons[currentSelectedBalloon.segment][currentSelectedBalloon.index]))
                        {
                            if (CheckBalloonIfHiddenPos(Balloons[item][i].PathPosition, tower))
                            {
                                currentSelectedBalloon = (item, i);
                            }
                        }
                    }
                }
            }
            if (!foundBalloon) // Sollte kein Ballon in der Reichweite gefunden worden sein
            {
                return (-1, -1);
            }

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
            foreach (Vector2D item in tower.NotVisibleSpots) // Alle nicht sichtbaren Stellen kontrollieren
            {
                if (item.X < balloonPos && item.Y > balloonPos) // Sollte der Ballon innerhalb einer der nicht sichtbaren Stellen sein wird abgebrochen
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Den übergebenen Turm lokal einfügen
        /// </summary>
        /// <param name="t"></param>
        private void AddTower(object t)
        {
            Towers.Add((Tower)t);
            Towers[Towers.Count - 1].SearchSegments(CurrentMap);
            if (!GodMode && !NetworkHandler.Resyncing) // Im GodMode kein Geld abziehen // Beim resynchronisieren auch kein Geld abziehen
            {
                Money -= (int)StaticInfo.GetTowerPrice(t.GetType());
            }
        }

        /// <summary>
        /// Fügt Türme in eine Liste ein wenn diese gezeichnet werden sollen aber noch inaktiv sind.
        /// Kontrolliert, ob Türme beim platzieren kollidieren.
        /// </summary>
        /// <param name="t"></param>
        private void AddVTower(object t)
        {
            if (StaticInfo.GetTowerPrice(GetType()) <= Money || GodMode) // Kontrollieren, dass der Spieler genug Geld hat
            {
                if (IsTowerValidPosition((t as Tower).Hitbox)) // Kontrollieren, dass der Turm an der Stelle platziert werden kann
                {
                    (t as Tower).IsSelected = false; // Ist nicht mehr der ausgewählte Turm
                    (t as Tower).IsPlaced = true;

                    VTowers.Add((Tower)t);
                }
                else
                {
                    for (int i = VTowers.Count - 1; i >= 0; i--)  //Überprüft, ob es eine Kollision mit einem noch nicht platzierten Turm gibt
                    {
                        if (VTowers[i].Hitbox.IntersectsWith((t as Tower).Hitbox)) // Turm finden mit dem der neu hinzugefügte kollidieren würde
                        {
                            if (VTowers[i].ActivateAtTick > (t as Tower).ActivateAtTick) // Kontrollieren, welcher der beiden Türme zuerst platziert wird und damit zuerst gebaut wurde
                            {
                                VTowers.RemoveAt(i); // Sollte der stehende Turm einfach vom Spieler platziert sein und es wird ein Paket erhalten welches vorher gesendet wurde wird das neu platzierte übernommen
                                VTowers.Add((Tower)t);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Die lokale Version des übergebenen Turmes entfernen
        /// </summary>
        /// <param name="t"></param>
        private void RemoveTower(object t)
        {
            Tower tower = FindTowerID(t);
            if (tower != null) // Kontrollieren, dass ein Turm existiert
            {
                Tower selectedTower = UILayout.TowerDetailsContainer.Context;
                if (selectedTower != null && selectedTower.ID == tower.ID) // Sollte der gelöschte Turm ausgewählt sein wird dieses Feld geschlossen
                {
                    UILayout.TowerDetailsContainer.Visible = false;
                }

                Money += (int)tower.SellPrice; // Geld fürs verkaufen an den Spieler auszahlen
                Towers.Remove(tower);
            }
        }

        public void UpdateHealth(object h)
        {
            HealthPoints = (int)h;
            if (HealthPoints <= 0) // Fragt ab ob der Spieler gerade verloren hat
                Program.LoadScreen(new GuiGameOver()); // Lade GuiGameOver falls dies der Fall ist
        }

        /// <summary>
        /// Upgraded die lokale Vesion des übergebenen Turmes
        /// </summary>
        /// <param name="t"></param>
        public void UpgradeTower(object t)
        {
            Tower tower = FindTowerID(t);
            if (tower != null && tower.UpgradePrice <= Money && tower.CanLevelUp()) // Kontrollieren, dass ein Turm existiert, dass der Spieler genug Geld hat und das der Turm weiter geupgraded werden kann
            {
                if (!GodMode) // Im Godmode soll kein Geld abgezogen werden
                {
                    Money -= (int)tower.UpgradePrice;
                }

                tower.Level++;
                tower.SearchSegments(CurrentMap); // Pfadsegmente in der Reichweite entsprechend der neuen Reichweite updaten
            }
        }

        /// <summary>
        /// Wechselt den Schussmodus der lokalen Version des übergebenen Turmes
        /// </summary>
        /// <param name="t"></param>
        public void ModeChangeTower(object t)
        {
            Tower tower = FindTowerID(t);
            if (tower != null) // Kontrollieren, dass ein Turm existiert
            {
                tower.TargetMode = (t as Tower).TargetMode;

                Tower selectedTower = UILayout.TowerDetailsContainer.Context; // Derzeit angezeigten Turm auswählen
                if (selectedTower != null && selectedTower.ID == tower.ID) // Sollte ein Turm ausgewählt sein und dem veränderten entsprechen wird die Ansicht geupdated
                {
                    UILayout.TowerDetailsContainer.Context = tower;
                }
            }
        }

        private void AddBalloon(object t)
        {
            Balloons[(t as Balloon).CurrentSegment].Add(t as Balloon);
        }

        public void StartRound(object t)
        {
            WaveManager.StartSpawn();
        }

        public void AccelerateGame(object t)
        {
            if (StaticEngine.TickAcceleration == 8)
            {
                StaticEngine.TickAcceleration = 1;
            }
            else
            {
                StaticEngine.TickAcceleration *= 2;
            }
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

        public void SendMessage(object t)
        {
            Messages.Add(t.ToString());
        }

        /// <summary>
        /// Findet den übergebenen Turm wenn dieser existiert 
        /// </summary>
        /// <param name="t"></param>
        /// <returns>Gibt einen Verweis auf das Objekt des übersendeten Turmes zurück</returns>
        private Tower FindTowerID(object t)
        {
            try // Falls kein Turm mehr gefunden wird, bsw bei mehrfachem Löschen soll das Program nicht crashen
            {
                foreach (Tower tower in Towers) // Alle Tower durchsuchen
                {
                    if (tower.ID == (t as Tower).ID)
                    {
                        return tower;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The towersearch failed with following message: " + e.Message);
            }
            return null;
        }
				
		private void TransferMousePosition(object m)
        {
			var networkPackage = m as NetworkPackageMousePosition;
			if (networkPackage == null // aus irgend einem Grund ist das schon mal passiert und hat zu Null reference Excep. geführt. Wenn sehr viele Events empfangen werden könnte es passieren
				|| networkPackage.Username == NetworkHandler.LocalPlayer.Name) return;

			bool hasFound = false;
			for (int i = 0; i < usersMousePos.Count; i++)
				if (usersMouseTag[i] == networkPackage.Username)
                {
					hasFound = true;
					if (usersMousePos[i].currentTick < networkPackage.currentTick)
						usersMousePos[i] =
							(networkPackage.Pos.X, networkPackage.Pos.Y, networkPackage.TTL + Environment.TickCount, networkPackage.currentTick);
				}
			if (!hasFound)
            {
				usersMousePos.Add(
					(networkPackage.Pos.X, networkPackage.Pos.Y, networkPackage.TTL + Environment.TickCount, networkPackage.currentTick));
				usersMouseTag.Add(networkPackage.Username);
			}
        }

        [Serializable]
        private class NetworkPackageMousePosition
        {
            public (int X, int Y) Pos = (0, 0);
            public string Username = String.Empty;
            public int currentTick = 0;
            public int TTL = 2000; // wird nicht überschrieben und in ms
        }
    }
}

