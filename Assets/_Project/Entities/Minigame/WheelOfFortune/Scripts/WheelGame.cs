namespace WheelOfFortune
{
	using Core;
	using Core.Utility;
	using GUI.Widgets;

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