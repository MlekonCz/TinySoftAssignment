using UnityEngine;

namespace Entities.Core.Scripts.GUI
{
	[CreateAssetMenu(fileName = "ScreenSettings", menuName = "Settings/ScreenSettings")]
	public class ScreenSettings : ScriptableObject
	{
		public ScreenView StartScreen;
		public ScreenView OptionsScreen;
	}
}