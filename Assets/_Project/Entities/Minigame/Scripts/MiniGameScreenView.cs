using System.Threading;
using Cysharp.Threading.Tasks;
using Entities.Core.Scripts;
using Entities.Core.Scripts.GUI;
using Entities.Core.Scripts.User;

namespace Entities.Minigame.Scripts
{
	public abstract class MiniGameScreenView<TSettings, TGame, TBet, TWinResult> : ScreenView<TGame> 
		where TSettings : MiniGameSettingsBase
		where TGame : MiniGameBase<TSettings, TBet, TWinResult> 
	{
		protected TGame m_Game;
		protected UserData m_UserData;
		private CancellationTokenSource m_cancellationTokenSource = new();

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
			PlayGameRoundTask(m_cancellationTokenSource.Token).Forget();
		}
		
		public void CancelExecution()
		{
			m_cancellationTokenSource.Cancel();
			m_cancellationTokenSource.Dispose();
			m_cancellationTokenSource = new CancellationTokenSource();
		}

		private async UniTask PlayGameRoundTask(CancellationToken cancellationToken)
		{
			var bet = GetRoundBet();
			
			if (m_Game.TryToPlaceBet(bet) == false)
				return;
			
			// block UI
			SetScreenBusy(true);

			var winResult = m_Game.GenerateWinResult();
			
			await AnimateGameRound(winResult, cancellationToken);
			
			m_Game.ProcessWinResult(bet, winResult);
			
			// unblock
			SetScreenBusy(false);
		}

		protected abstract TBet GetRoundBet(); 
		
		protected virtual UniTask AnimateGameRound(TWinResult winResult, CancellationToken cancellationToken)
		{
			return UniTask.CompletedTask;
		}
		
		protected virtual void HandleBetChanged()
		{
		}
	}
}