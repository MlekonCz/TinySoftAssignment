using Entities.Core.Scripts.User;
using Entities.Core.Scripts.Utility;
using Entities.Minigame.Scripts;

namespace Entities.Minigame.WheelOfFortune.Scripts
{
	public class WheelGame : MiniGameBase<WheelGameSettings, long, WheelWidget.WheelSegmentConfig>
	{
		public WheelGame(WheelGameSettings settings, UserService userService) : base(settings, userService)
		{
			
		}

		public override bool TryToPlaceBet(long bet)
		{
			if (m_UserData.Balance < bet)
				return false;

			m_UserData.AddBalance(-bet);
			return true;
		}

		public override WheelWidget.WheelSegmentConfig GenerateWinResult()
		{
			// TODO: Fix SelectWeightedRandom method, which doesn't use weights at the moment!
			return Settings.WheelConfig.Segments.SelectWeightedRandom(m_Random);
		}

		public override void ProcessWinResult(long bet, WheelWidget.WheelSegmentConfig winResult)
		{
			m_UserData.AddBalance(bet * winResult.WinPercent / 100);
		}
	}
}