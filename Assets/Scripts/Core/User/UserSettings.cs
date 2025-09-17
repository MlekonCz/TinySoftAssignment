namespace Core
{
	using UnityEngine;

	[CreateAssetMenu(fileName = "UserSettings", menuName = "Settings/UserSettings")]
	public class UserSettings : ScriptableObject
	{
		public long StartBalance = 10000;
	}
}