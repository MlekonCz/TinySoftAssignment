namespace Core
{
	using UnityEngine;

	[CreateAssetMenu(fileName = "ScreenSettings", menuName = "Settings/ScreenSettings")]
	public class ScreenSettings : ScriptableObject
	{
		public ScreenView StartScreen;
		public ScreenView OptionsScreen;
	}
}