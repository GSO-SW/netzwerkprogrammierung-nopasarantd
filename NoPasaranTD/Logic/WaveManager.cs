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

        public WaveManager(Game game, int numberBalloon)
        {
            CurrentGame = game;

            currentWave = GetNextBalloonWave(numberBalloon);
            currentWavePackage = GetNewBalloonPackage();
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
        private int currentBalloonOfPackage = 0;

        // Die derzeitigen Ballons der Welle
        private List<Balloon> currentWave;

        // Die derzeitige Anzahl an genutzten Ballons der Welle
        private int currentBalloonOfWave = 0;

        // Wellen-Parameter
        private readonly double waveSensitivity = 0.0015; // Die Sensitivität der Wellen
        private readonly double balloonStartValue = 50; // Die Anzahl der Ballons, die zum Beginn Spawnen
        private readonly uint waveSensitivityExponent = 2; // Der Exponent der den Spawn-Grad verändern kann

        private bool completed = false;
        public bool IsCompleted
        {
            get => completed;
            set
            {
                completed = value;
                if (value)
                {
                    CurrentGame.Round++;
                }
            }
        }

        public bool IsRoundCompleted => completed && CheckIsBalloonsEmpty();

        private List<Balloon> GetNextBalloonWave(int numberBalloons)
        {
            int currentBalloons = 0;
            Balloon[] balloons = new Balloon[numberBalloons];

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
            {
                probabilities[i - 1] = (probabilities[i - 1].Item1, probabilities[i - 1].Item2 * (1 / sumProbability));
            }

            // Fügt die Ballons hinzu
            for (int i = 0; i < probabilities.Length; i++)
            {
                for (int j = 0; j < (int)(probabilities[i].Item2 * numberBalloons); j++)
                {
                    balloons[currentBalloons] = new Balloon(probabilities[i].Item1);
                    currentBalloons++;
                }
            }

            List<Balloon> balloonList = new List<Balloon>(balloons);
            balloonList.RemoveAll(x => x == null);
            return balloonList;
        }

        // Berechnet die Häufigkeit eines Ballontypes innerhalb einer Welle
        private double CalcProberbility(BalloonType type)
        {
            // Funktion: e^(-0.05(x-a-5)*(x-a+5)-1.25) | a=x-Wert an dem nur dieser Ballontyp spawnt
            uint peek = StaticInfo.GetBalloonPeek(type);
            return Math.Exp(-0.05 * (CurrentGame.Round - peek - 5) * (CurrentGame.Round - peek + 5) - 1.25);
        }

        // Erstellt ein neues Paket an Ballons innerhalb einer Welle
        private List<Balloon> GetNewBalloonPackage()
        {
            List<Balloon> newWave = new List<Balloon>(); // Die neue Paket Welle

            // Errechnet einen neuen Wert zwischen 0 und 1 welches dann der Anteil der gesamten Welle ist und erstellt diese Menge an Ballons dann
            double part = -0.25 * Math.Sin(-0.4 * Math.Log(Math.Pow((CurrentGame.Round + currentBalloonOfWave / currentWave.Count), Math.Cos(CurrentGame.Round + currentBalloonOfWave / currentWave.Count)))) + 0.4;
            if (currentBalloonOfWave + currentWave.Count * part < currentWave.Count)
            {
                newWave = currentWave.GetRange(currentBalloonOfWave + 1, (int)(currentWave.Count * part));
                currentBalloonOfPackage = 0;
            }
            else if (currentBalloonOfWave + currentWave.Count * part > currentWave.Count)
            {
                newWave = currentWave.GetRange(currentBalloonOfWave + 1, currentWave.Count - currentBalloonOfWave - 1);
                currentBalloonOfPackage = 0;
            }
            return newWave;
        }

        // Übergibt die neue Anzahl an Ballons in einer Runde in Abhängigkeit der Runde, dem Startwert, der Intensitivität und dem Exponenten
        private int GetBalloonCountInRound()
        {
            return (int)(balloonStartValue * (waveSensitivity * Math.Pow(CurrentGame.Round, waveSensitivityExponent) + 1));
        }

        /// <summary>
        /// Checkt ob im laufenden Spiel noch Balloons vorhanden sind
        /// </summary>
        /// <returns></returns>
        private bool CheckIsBalloonsEmpty()
        {
            for (int i = 0; i < CurrentGame.Balloons.Length; i++)
            {
                if (CurrentGame.Balloons[i].Count != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Update Methode des Ballon-Wave Managers
        /// </summary>
        public void Update()
        {
            int currentWaitTime;

            if (currentBalloonOfPackage == 0)
            {
                currentWaitTime = 2800;
            }
            else
            {
                currentWaitTime = 200;
            }

            // Setzt eine neue Welle falls die derzeitige bereits vorüber ist
            if (currentWave.Count - 1 == currentBalloonOfWave)
            {
                currentWave = GetNextBalloonWave(GetBalloonCountInRound());
                currentBalloonOfWave = 0;
                currentBalloonOfPackage = 0;
                currentWavePackage = GetNewBalloonPackage();
                IsCompleted = true;
            }

            if (!IsCompleted)
            {
                // Setzt ein neues Paket an Ballons falls das derzeitige bereits genutzt wurde
                if (currentBalloonOfPackage == currentWavePackage.Count - 1)
                {
                    currentWavePackage = GetNewBalloonPackage();
                    currentBalloonOfPackage = 0;
                }

                // Setzt einen Ballon an den Spawnpoint
                if (CurrentGame.CurrentTick % currentWaitTime == 0)
                {
                    CurrentGame.Balloons[0].Add(currentWavePackage[currentBalloonOfPackage]);
                    currentBalloonOfPackage++;
                    currentBalloonOfWave++;
                }
            }
            else if (CheckIsBalloonsEmpty())
            {
                WaveCompleted?.Invoke();
                if (AutoStart)
                {
                    IsCompleted = false;
                }
            }
        }

        /// <summary>
        /// Setzt das Spawnen von Ballons fort
        /// </summary>
        public void StartSpawn()
        {
            if (IsCompleted)
				IsCompleted = false;
        }

        /// <summary>
        /// Stopt das Spawnen von neuen Ballons
        /// </summary>
        public void StopSpawn()
        {
            if (!IsCompleted)
				IsCompleted = true;
        }		
	}
}
