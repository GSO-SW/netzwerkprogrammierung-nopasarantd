using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NoPasaranTD.Engine
{
	class Game
	{
		uint tickCount;
		Random random = new Random();

		public Map CurrentMap { get; }
		public List<Balloon> Balloons { get; }
		public List<Tower> Towers { get; }

		
		

	  
		private void SpawnSetBallon() 
		{
			
			
            if (tickCount == 1000)
            {
				Balloon balloon = new Balloon
			    {
				  PathPosition = 0,
				
			    };
			    Array values = Enum.GetValues(typeof(BalloonType));
			    balloon.Type = (BalloonType)values.GetValue(random.Next(1,values.Length));
				
				
				Balloons.Add(balloon);

            }			
		}

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
			SpawnSetBallon();
			tickCount++;
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
