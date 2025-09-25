using System;
using Entities.Core.Scripts.GUI;
using Entities.Core.Scripts.User;
using Entities.View.Scripts.Widgets;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Entities.Minigame.Scripts
{
	public abstract class MiniGameSettingsBase : ScriptableObject
	{
		public string GameName;
		public string UniqueId;
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
        private const string KEY = "test_data";
		
		[Button]
		private void SimulateRounds(TBet bet, long startBalance = 10000, int playRoundsCount = 10000)
		{
			var userService = new UserService(new PlayerPrefsSaveRep(KEY));
			var userData = userService.UserData; 
			userData.SetBalance(startBalance);
			
			var game = CreateTypedGame(userService);
			var gameCounter = 0;

			long totalStake = 0;
			long totalPayout = 0;
			var prevBalance = userData.Balance;

			for (var i = 0; i < playRoundsCount; i++)
			{
				if (!game.TryToPlaceBet(bet))
				{
					Debug.Log("Simulation ended early.");
					break;
				}

				gameCounter++;

				var afterBet = userData.Balance;
				totalStake += Math.Max(0, prevBalance - afterBet);


				var winResult = game.GenerateWinResult();
				game.ProcessWinResult(bet, winResult);

				var afterWin = userData.Balance;
				totalPayout += Math.Max(0, afterWin - afterBet);
				prevBalance = afterWin;
			}

			var rtp = (double)(userData.Balance - startBalance + totalStake) / totalStake;
			Debug.Log($"Played {gameCounter:N} games, balance progress {startBalance:N} => {userData.Balance:N}. RTP: {(rtp):N}%");
		}
#endif
#endregion
	}	
}