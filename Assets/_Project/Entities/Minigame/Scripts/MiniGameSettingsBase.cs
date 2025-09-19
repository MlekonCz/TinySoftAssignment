namespace Core
{
	using GUI.Widgets;
	using Sirenix.OdinInspector;
	using UnityEngine;

	public abstract class MiniGameSettingsBase : ScriptableObject
	{
		public string GameName;
		public GameThumbnail GameThumbnail;

		public abstract ScreenView CreateAndOpenGameView(UserService userService, ScreenStack screenStack);
	}


	// Generically Typed GameSettings - Defined types provides a common way to start a game, open a GameScreenView or handle game Simulation
	
	public abstract class MiniGameSettingsBase<TSettings, TGame, TScreenView, TBet, TWinItem> : MiniGameSettingsBase 
		where TSettings : MiniGameSettingsBase 
		where TGame : MiniGameBase<TSettings, TBet, TWinItem> 
		where TScreenView : MiniGameScreenView<TSettings, TGame, TBet, TWinItem> 
		where TWinItem : IWinResult
	{
		public TScreenView GameScreenPrefab;
		
		public override ScreenView CreateAndOpenGameView(UserService userService, ScreenStack screenStack)
		{
			var game = CreateTypedGame(userService);
			return screenStack.OpenScreen(GameScreenPrefab, game);
		}
		
		protected abstract TGame CreateTypedGame(UserService userService);

		
#region Simulation
#if UNITY_EDITOR
		[Button]
		private void SimulateRounds(TBet bet, long startBalance = 10000, int playRoundsCount = 10000)
		{
			var userService = new UserService();
			var userData = userService.UserData; 
			userData.SetBalance(startBalance);
			
			var game = CreateTypedGame(userService);
			var gameCounter = 0;
			for (int i = 0; i < playRoundsCount; i++)
			{
				if (game.TryToPlaceBet(bet) == false)
				{
					Debug.Log($"Simulation ended early.");
					break;
				}
				
				gameCounter++;
				var winResult = game.GenerateWinResult();
				game.ProcessWinResult(bet, winResult);
			}
			Debug.Log($"Played {gameCounter:N} games, balance progress {startBalance:N} => {userData.Balance:N}. RTP: ???");
		}
#endif
#endregion
	}	
}