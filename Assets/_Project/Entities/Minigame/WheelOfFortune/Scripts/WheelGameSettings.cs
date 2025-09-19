namespace WheelOfFortune
{
	using Core;
	using GUI.Widgets;
	using UnityEngine;

	[CreateAssetMenu(fileName = "WheelGameSettings", menuName = "Settings/Games/WheelGameSettings")]
	public class WheelGameSettings : MiniGameSettingsBase<WheelGameSettings, WheelGame, WheelGameScreen, long, WheelWidget.WheelSegmentConfig>
	{
		public WheelWidget.WheelConfig WheelConfig;

		protected override WheelGame CreateTypedGame(UserService userService)
		{
			return new WheelGame(this, userService);
		}
	}	
}