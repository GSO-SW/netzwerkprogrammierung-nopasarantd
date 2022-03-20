using NoPasaranTD.Data;
using NoPasaranTD.Model;
using System;
using System.Collections.Generic;

namespace NoPasaranTD.Engine
{
	/// <summary>
	/// Der Wellen-Manager bestimmt wann ein Ballon zu welcher Runde und in welcher Häufigkeit spawnen soll
	/// </summary>
    public class WaveManager
    {
		/// <summary>
		/// Event das beim vollständigen Wellen Abschluss ausgelöst wird
		/// </summary>
		public event Action WaveCompleted;

		/// <summary>
		/// Das derzeitige Spiel
		/// </summary>
		public Game CurrentGame { get; set; }
		
		/// <summary>
		/// Soll nach Wellenabschluss eine neue Welle automatisch generiert werden?
		/// </summary>
		public bool AutoStart { get; set; } = false;

        public WaveManager(Game game, int numberBallon)
        {
			CurrentGame = game;

			currentWave = GetNextBallonWave(numberBallon);
			currentWavePackage = GetNewBallonPackage(currentWave);
		}

		/// <summary>
		/// Erkärung des Spawnalgorithmus graphisch:
		/// 
		///							   -Welle-
		///  Paket            Paket		     Paket		  Paket
		/// . . . . .     . . . . . .      . . . . .    . . . . . .
		/// 
		/// </summary>

		// Das derzeitige zu Spawnende Paket
		private List<Balloon> currentWavePackage;

		// Die Anzahl an genutzten Ballons eines Paketes
		private int currentBallonOfPackage = 0;

		// Die derzeitigen Ballons der Welle
		private List<Balloon> currentWave;

		// Die derzeitige Anzahl an genutzten Ballons der Welle
		private int currentBallonOfWave = 0;
		
		// Wellen-Parameter
		private double waveSensitivity = 0.0015; // Die Sensitivität der Wellen
		private double ballonStartValue = 50; // Die Anzahl der Ballons, die zum Beginn Spawnen
		private uint waveSensitivityExponent = 2; // Der Exponent der den Spawn-Grad verändern kann
		
		private bool isCompleted = false;
		public bool IsCompleted
        {
			get { return isCompleted; }
			set 
			{ 
				isCompleted = value;
                if (value)
                {
					CurrentGame.Round++;
                }
			}
        }
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

		// Erstellt ein neues Paket an Ballons innerhalb einer Welle
		private List<Balloon> GetNewBallonPackage(List<Balloon> ballons)
		{
			List<Balloon> newWave = new List<Balloon>(); // Die neue Paket Welle

			// Errechnet einen neuen Wert zwischen 0 und 1 welches dann der Anteil der gesamten Welle ist und erstellt diese Menge an Ballons dann
			double part = -0.25 * Math.Sin(-0.4 * Math.Log(Math.Pow((CurrentGame.Round + currentBallonOfWave / currentWave.Count),Math.Cos(CurrentGame.Round + currentBallonOfWave / currentWave.Count)))) + 0.4;
            if (currentBallonOfWave + currentWave.Count * part < currentWave.Count)
            {
				newWave = currentWave.GetRange(currentBallonOfWave + 1, (int)(currentWave.Count * part));
				currentBallonOfPackage = 0;
            }
			else if (currentBallonOfWave + currentWave.Count * part > currentWave.Count)
            {
				newWave = currentWave.GetRange(currentBallonOfWave + 1, currentWave.Count - currentBallonOfWave-1);
				currentBallonOfPackage = 0;
			}
			return newWave;
		}

		// Übergiebt die neue Anzahl an Ballons in einer Runde in Abhängigkeit der Runde, dem Startwert, der Intensitivität und dem Exponenten
		private int GetBallonNumberInRound() => (int)(ballonStartValue * (waveSensitivity * Math.Pow(CurrentGame.Round, waveSensitivityExponent)+1));

		/// <summary>
		/// Checkt ob im laufenden Spiel noch Balloons vorhanden sind
		/// </summary>
		/// <returns></returns>
		private bool CheckIsBallonsEmpty()
		{
			for (int i = 0; i < CurrentGame.Balloons.Length; i++)
				if (CurrentGame.Balloons[i].Count != 0)
					return false;

			return true;
		}

		/// <summary>
		/// Update Methode des Ballon-Wave Managers
		/// </summary>
		public void Update()
        {
			int currentWaitTime;

			if (currentBallonOfPackage == 0)
				currentWaitTime = 2800;
			else
				currentWaitTime = 200;
				
			// Setzt eine neue Welle falls die derzeitige bereits vorüber ist
			if (currentWave.Count - 1 == currentBallonOfWave)
			{				
				currentWave = GetNextBallonWave(GetBallonNumberInRound());
				currentBallonOfWave = 0;
				currentBallonOfPackage = 0;
				currentWavePackage = GetNewBallonPackage(currentWave);
				IsCompleted = true;
			}

			if (!IsCompleted)
			{
				// Setzt ein neues Paket an Ballons falls das derzeitige bereits genutzt wurde
				if (currentBallonOfPackage == currentWavePackage.Count - 1)
				{
					currentWavePackage = GetNewBallonPackage(currentWave);
					currentBallonOfPackage = 0;
				}

				// Setzt einen Ballon an den Spawnpoint
				if (CurrentGame.CurrentTick % currentWaitTime == 0)
				{
					CurrentGame.Balloons[0].Add(currentWavePackage[currentBallonOfPackage]);
					currentBallonOfPackage++;
					currentBallonOfWave++;
				}
			}
			else if (CheckIsBallonsEmpty())
            {
				WaveCompleted?.Invoke();
                if (AutoStart)
					IsCompleted = false;
            }							
		}
	
		/// <summary>
		/// Setzt das Spawnen von Ballons fort
		/// </summary>
		public void StartSpawn()
        {
            if (IsCompleted == true)
				IsCompleted = false;
        }

		/// <summary>
		/// Stopt das Spawnen von neuen Ballons
		/// </summary>
		public void StopSpawn()
        {
            if (isCompleted == false)
				isCompleted = true;
        }		
	}
}
