using Entities.Core.Scripts.GUI;
using Entities.Core.Scripts.User;
using Entities.Minigame.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Entities.Core.Scripts
{
	[CreateAssetMenu(fileName = "MasterSettings", menuName = "Settings/Settings")]
	[InlineEditor]
	public class MasterSettings : ScriptableObject
	{
		public UserSettings			UserSettings;
		public ScreenSettings		ScreenSettings;
		public MiniGamesSettings	MiniGamesSettings;
	}
}