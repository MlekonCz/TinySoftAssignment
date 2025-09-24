using System.Collections.Generic;
using UnityEngine;

namespace Entities.Minigame.Scripts
{
	[CreateAssetMenu(fileName = "MiniGamesSettings", menuName = "Settings/GameListSettings")]
	public class MiniGamesSettings : ScriptableObject
	{
		public List<long> BetAmounts = new List<long>() { 100, 500, 1000, 2000, 3000, 5000, 10000 };

		public List<MiniGameSettingsBase> MiniGameList = new List<MiniGameSettingsBase>();
	}
}