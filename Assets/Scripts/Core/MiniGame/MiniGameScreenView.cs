namespace Core
{
	using System.Threading.Tasks;

	public abstract class MiniGameScreenView<TSettings, TGame, TBet, TWinResult> : ScreenView<TGame> 
		where TSettings : MiniGameSettingsBase
		where TGame : MiniGameBase<TSettings, TBet, TWinResult> 
	{
		protected TGame m_Game;
		protected UserData m_UserData;

		internal override void Initialize(ScreenStack stack, ServiceLocator locator)
		{
			base.Initialize(stack, locator);

			m_UserData = locator.Get<UserService>().UserData;
			m_UserData.SignalBetIndexChanged += HandleBetChanged;
		}

		internal override void Deinitialize()
		{
			base.Deinitialize();

			m_UserData.SignalBetIndexChanged -= HandleBetChanged;
		}

		public override void BindScreenData(TGame game)
		{
			m_Game = game;
		}

		internal override void OnShow()
		{
			base.OnShow();
			
			m_Game.Start();
		}

		internal override void OnHide()
		{
			base.OnHide();
			
			m_Game.Dispose();
			m_Game = null;
		}

		// Main function for playing one round of "Game"
		protected void PlayGameRound()
		{
			PlayGameRoundTask();
		}

		private async Task PlayGameRoundTask()
		{
			var bet = GetRoundBet();
			
			if (m_Game.TryToPlaceBet(bet) == false)
				return;
			
			// block UI
			SetScreenBusy(true);

			var winResult = m_Game.GenerateWinResult();
			
			await AnimateGameRound(winResult);
			
			m_Game.ProcessWinResult(bet, winResult);
			
			// unblock
			SetScreenBusy(false);
		}

		protected abstract TBet GetRoundBet(); 

		protected virtual Task AnimateGameRound(TWinResult winResult)
		{
			return Task.CompletedTask;
		}
		
		protected virtual void HandleBetChanged()
		{
		}
	}
}