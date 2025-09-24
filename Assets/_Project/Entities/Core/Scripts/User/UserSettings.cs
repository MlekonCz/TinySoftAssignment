using UnityEngine;

namespace Entities.Core.Scripts.User
{
	[CreateAssetMenu(fileName = "UserSettings", menuName = "Settings/UserSettings")]
	public class UserSettings : ScriptableObject
	{
		public long StartBalance = 10000;
	}
}