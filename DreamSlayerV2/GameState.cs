using System;
using DreamSlayerV2.Scenes;

namespace DreamSlayerV2.Core
{
	public static class GameState
	{
		private static PlayerController _currentPlayer;

		public static int CurrentDepth { get; set; } = 1;
		public static int NodesVisited { get; private set; }
		public static int EnemiesDefeated { get; private set; }
		public static int CardsPlayed { get; private set; }
		public static int DepthsCleared { get; private set; }

		public static PlayerController CurrentPlayer
		{
			get
			{
				if (_currentPlayer == null)
				{
					_currentPlayer = new PlayerController(CharacterClassV2.Human);
				}
				return _currentPlayer;
			}
			set
			{
				_currentPlayer = value;
			}
		}

		public static void ResetRun()
		{
			CurrentDepth = 1;
			NodesVisited = 0;
			EnemiesDefeated = 0;
			CardsPlayed = 0;
			DepthsCleared = 0;
		}

		public static void RestoreRun(SavedRunData run)
		{
			CurrentDepth = Math.Max(1, run.CurrentDepth);
			NodesVisited = Math.Max(0, run.NodesVisited);
			EnemiesDefeated = Math.Max(0, run.EnemiesDefeated);
			CardsPlayed = Math.Max(0, run.CardsPlayed);
			DepthsCleared = Math.Max(0, run.DepthsCleared);
		}

		public static void RecordNodeVisited() => NodesVisited++;
		public static void RecordEnemyDefeated() => EnemiesDefeated++;
		public static void RecordCardPlayed() => CardsPlayed++;

		public static void CompleteDepth()
		{
			DepthsCleared++;
			UpgradeOrbPermanent.TotalDepthsCleared++;
			if (CurrentPlayer.IsNightmareMode)
			{
				UpgradeOrbPermanent.AscendedCoins++;
			}

			GameSaveManager.SavePermanentProgress();
		}

		public static int CalculateScore(bool escaped)
		{
			int score = CalculateBaseScore();

			if (escaped && CurrentPlayer != null)
			{
				score += Math.Max(0, CurrentPlayer.PlayerHP) * 20;
				score += Math.Max(0, CurrentPlayer.Sanity) * 10;
			}
			else
			{
				score -= 100;
			}

			return CurrentPlayer?.IsNightmareMode == true ? score * 2 : score;
		}

		public static int CalculateCurrentScore()
		{
			int score = CalculateBaseScore();
			return CurrentPlayer?.IsNightmareMode == true ? score * 2 : score;
		}

		private static int CalculateBaseScore()
		{
			return (NodesVisited * 100) +
				(EnemiesDefeated * 250) +
				(CardsPlayed * 10) +
				(DepthsCleared * 1000);
		}
	}
}