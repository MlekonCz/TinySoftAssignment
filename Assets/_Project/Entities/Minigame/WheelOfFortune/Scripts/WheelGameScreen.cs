using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

namespace WheelOfFortune
{
	using System;
	using System.Threading.Tasks;
	using Core;
	using GUI.Widgets;
	using UnityEngine;
	using Random = UnityEngine.Random;

	public class WheelGameScreen : MiniGameScreenView<WheelGameSettings, WheelGame, long, WheelWidget.WheelSegmentConfig>
	{
		[SerializeField] private TextMeshProUGUI m_GameName;
		[SerializeField] private BetAndPlayWidget m_BetAndPlayWidget;
		[SerializeField] private WheelWidget m_WheelWidget;

		internal override void Initialize(ScreenStack stack, ServiceLocator locator)
		{
			base.Initialize(stack, locator);

			if (m_BetAndPlayWidget == null)
				throw new Exception("Game needs Bet and Play Widget!");
				
			// Connect game starting event
			m_BetAndPlayWidget.SignalPlayClicked += PlayGameRound;
		}

		internal override void Deinitialize()
		{
			base.Deinitialize();
			
			m_BetAndPlayWidget.SignalPlayClicked -= PlayGameRound;
		}
		
		public override void BindScreenData(WheelGame game)
		{
			base.BindScreenData(game);

			if (m_GameName != null)
			{
				m_GameName.text = game.Settings.GameName;
			}
			
			m_WheelWidget.SetupSegments(game.Settings.WheelConfig);
			m_WheelWidget.SetRotationToSegment(Random.Range(0, game.Settings.WheelConfig.Segments.Count));
			
			SetupWheelWins();
		}

		protected override long GetRoundBet()
		{
			return m_BetAndPlayWidget.BetValue;
		}

		protected override async UniTask AnimateGameRound(WheelWidget.WheelSegmentConfig winItem, CancellationToken cancellationToken)
		{
			var winIndex = m_Game.Settings.WheelConfig.Segments.IndexOf(winItem);

			await m_WheelWidget.AnimateRotationToSegment(winIndex,cancellationToken);
			
			Debug.Log("Rotation finished");
		}

		protected override void HandleBetChanged()
		{
			base.HandleBetChanged();
			
			SetupWheelWins();
		}

		// Sets wheel texts to display real win amounts.
		private void SetupWheelWins()
		{
			foreach (var segment in m_WheelWidget.WheelSegments)
			{
				segment.SetText($"{m_BetAndPlayWidget.BetValue * segment.Config.WinPercent / 100:N0}");
			}
		}
	}
}