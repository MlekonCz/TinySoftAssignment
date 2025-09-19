namespace Core
{
	using Sirenix.OdinInspector;
	using UnityEngine;

	public class UserService : IInitializable, IUpdatable
	{
		[ShowInInspector]
		public UserData UserData { get; private set; } = new UserData();

		private UserSettings m_UserSettings;

		void IInitializable.Initialize(ServiceLocator locator)
		{
			m_UserSettings = locator.Get<MasterSettings>().UserSettings;
			SetStartingUserData();
		}

		void IInitializable.Deinitialize()
		{
		}

		void IUpdatable.Update(float deltaTime)
		{
			if (UserData.Dirty)
			{
				Debug.Log("User data changed!");
				UserData.ClearDirty();
			}
		}

		public void ResetUser()
		{
			SetStartingUserData();
		}

		private void SetStartingUserData()
		{
			UserData.SetBalance(m_UserSettings.StartBalance);
		}
	}
}