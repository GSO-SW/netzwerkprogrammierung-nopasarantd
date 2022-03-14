using NoPasaranTD.Data;
using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Engine
{
    public class WaveManager
    {
		public Game CurrentGame { get; set; }
        public List<Balloon> CurrentWavePackage { get; set; }
        public int CurrentBallonOfPackage = 0;

        public WaveManager(Game game, int numberBallon)
        {
			CurrentGame = game;
			currentWave = GetNextBallonWave(numberBallon);
        }

		private List<Balloon> currentWave;
		private int currentBallonOfWave = 0;
		private List<Balloon> GetNextBallonWave(int numberBallons)
		{
			int currentBallons = 0;
			Balloon[] ballons = new Balloon[numberBallons];

			// Häufigkeiten eines Ballontypes
			(BalloonType, double)[] probabilities = new (BalloonType, double)[Enum.GetNames(typeof(BalloonType)).Length];

			// Alle möglichen Ballontypen
			BalloonType[] values = (BalloonType[])Enum.GetValues(typeof(BalloonType));

			// Summe aller Häufigkeiten
			double sumProbability = 0;

			// Berechnet die Häufigkeit eines Ballontypes innerhalb einer Welle 
			for (int i = 1; i < Enum.GetNames(typeof(BalloonType)).Length; i++)
			{
				double probability = CalcProberbility(values[i]);

				probabilities[i - 1] = (values[i], CalcProberbility(values[i]));
				sumProbability += probability;
			}

			// Berechnet den Anteil eines Ballontypes innerhalb einer Welle
			for (int i = 1; i < Enum.GetNames(typeof(BalloonType)).Length; i++)
				probabilities[i - 1] = (probabilities[i - 1].Item1, probabilities[i - 1].Item2 * (1 / sumProbability));

			// Fügt die Ballons hinzu
			for (int i = 0; i < probabilities.Length; i++)
				for (int j = 0; j < (int)(probabilities[i].Item2 * numberBallons); j++)
				{
					ballons[currentBallons] = new Balloon(probabilities[i].Item1);
					currentBallons++;
				}


			List<Balloon> ballonsList = new List<Balloon>(ballons);
			ballonsList.RemoveAll(x => x == null);

			return ballonsList;
		}

		// Berechnet die Häufigkeit eines Ballontypes innerhalb einer Welle
		double CalcProberbility(BalloonType type)
		{
			// Funktion: e^(-0.05(x-a-5)*(x-a+5)-1.25) | a=x-Wert an dem nur dieser Ballontyp spawnt
			uint peek = StaticInfo.GetBallonPeek(type);
			return Math.Exp(-0.05 * (CurrentGame.Round - peek - 5) * (CurrentGame.Round - peek + 5) - 1.25);
		}

		private List<Balloon> GetNewBallonPackage(List<Balloon> ballons)
		{
			double part = -0.25 * Math.Sin(-0.4 * Math.Log(CurrentGame.Round + currentBallonOfWave / currentWave.Count)) + 0.4;
            if (currentBallonOfWave + currentWave.Count * part < currentWave.Count)
            {
				CurrentWavePackage = currentWave.GetRange(currentBallonOfWave + 1, (int)(currentWave.Count * part));
				CurrentBallonOfPackage = 0;
            }
			else if (currentBallonOfWave + currentWave.Count * part > currentWave.Count)
            {
				CurrentWavePackage = currentWave.GetRange(currentBallonOfWave + 1, currentWave.Count - currentBallonOfWave-1);
				CurrentBallonOfPackage = 0;
			}
			return ballons;
		}

		public void Update()
        {
			int currentWaitTime = 150;

			if (CurrentWavePackage == null || CurrentBallonOfPackage == CurrentWavePackage.Count - 1)
			{
				CurrentWavePackage = GetNewBallonPackage(currentWave);
			}

			if (CurrentBallonOfPackage == 0)
				currentWaitTime = 1000;
			else
				currentWaitTime = 150;

			if (CurrentGame.CurrentTick % currentWaitTime == 0)
			{
				CurrentGame.Balloons[0].Add(CurrentWavePackage[CurrentBallonOfPackage]);
				CurrentBallonOfPackage++;
				currentBallonOfWave++;
			}

			if (CurrentWavePackage.Count - 1 == CurrentBallonOfPackage)
			{
				CurrentGame.Round++;
				currentWave = GetNextBallonWave((int)(100));
				currentBallonOfWave = 0;
			}
		}
	}
}
