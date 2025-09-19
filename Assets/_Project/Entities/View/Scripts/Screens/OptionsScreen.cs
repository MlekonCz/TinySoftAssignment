namespace GUI
{
	using Core;
	using UnityEngine;
	using UnityEngine.UI;

	public class OptionsScreen : ScreenView
	{
		[SerializeField] private Button m_ResetUserButton;
		
		internal override void Initialize(ScreenStack stack, ServiceLocator locator)
		{
			base.Initialize(stack, locator);

			m_ResetUserButton.onClick.AddListener(HandleResetUserClick);
		}

		internal override void Deinitialize()
		{
			base.Deinitialize();
			
			m_ResetUserButton.onClick.RemoveListener(HandleResetUserClick);
		}

		private void HandleResetUserClick()
		{
			// TODO: Reset user data
			Debug.Log("TODO: Reset UserData");
		}
	}
}