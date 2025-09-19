namespace Core
{
	using Sirenix.OdinInspector;
	using UnityEngine;

	[CreateAssetMenu(fileName = "MasterSettings", menuName = "Settings/Settings")]
	[InlineEditor]
	public class MasterSettings : ScriptableObject
	{
		public UserSettings			UserSettings;
		public ScreenSettings		ScreenSettings;
		public MiniGamesSettings	MiniGamesSettings;
	}
}