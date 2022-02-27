using NoPasaranTD.Model;
using System;
using System.Collections.Generic;

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
	}
}
