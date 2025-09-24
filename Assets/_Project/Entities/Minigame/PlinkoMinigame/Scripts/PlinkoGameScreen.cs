using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using System;
using Core;
using GUI.Widgets;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Entities.Minigame.PlinkoMinigame.Scripts
{
    public class PlinkoGameScreen : MiniGameScreenView<PlinkoGameSettings, PlinkoGame, long, PlinkoWidget.PlinkoBoxConfig>
    {
        [SerializeField] private TextMeshProUGUI m_GameName;
        [SerializeField] private BetAndPlayWidget m_BetAndPlayWidget;
        [SerializeField] private PlinkoWidget m_PlinkoWidget;

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
		
        public override void BindScreenData(PlinkoGame game)
        {
            base.BindScreenData(game);

            if (m_GameName != null)
            {
                m_GameName.text = game.Settings.GameName;
            }
			
            m_PlinkoWidget.SetupSegments(game.Settings.PlinkoConfig);
			
            SetupWheelWins();
        }

        protected override long GetRoundBet()
        {
            return m_BetAndPlayWidget.BetValue;
        }

        protected override async UniTask AnimateGameRound(PlinkoWidget.PlinkoBoxConfig winItem, CancellationToken cancellationToken)
        {
            var winIndex = m_Game.Settings.PlinkoConfig.Box.IndexOf(winItem);

            await m_PlinkoWidget.AnimateRotationToSegment(winIndex,cancellationToken);
        }

        protected override void HandleBetChanged()
        {
            base.HandleBetChanged();
			
            SetupWheelWins();
        }

        // Sets wheel texts to display real win amounts.
        private void SetupWheelWins()
        {
            foreach (var segment in m_PlinkoWidget.BoxWidgets)
            {
                segment.SetText($"{m_BetAndPlayWidget.BetValue * segment.Config.WinPercent / 100:N0}");
            }
        }
    }
}