namespace DreamSlayerV2.Core
{
	public static class GameState
	{
		private static PlayerController _currentPlayer;

		public static PlayerController CurrentPlayer
		{
			get
			{
				if (_currentPlayer == null)
				{
					_currentPlayer = new PlayerController(CharacterClassV2.HumanDemo);
				}
				return _currentPlayer;
			}
			set
			{
				_currentPlayer = value;
			}
		}
	}
}