namespace GUI.Widgets
{
	using Core;
	using UnityEngine;
	using UnityEngine.UI;

	public class OpenSettingsButton : ScreenWidget
	{
		[SerializeField] private Button m_Button;
		
		private ScreenView m_OptionsScreenPrefab;

		public override void Initialize(ScreenStack stack, ScreenView owner, ServiceLocator locator)
		{
			base.Initialize(stack, owner, locator);

			m_OptionsScreenPrefab = locator.Get<MasterSettings>().ScreenSettings.OptionsScreen;
			
			m_Button.onClick.AddListener(HandleButtonClick);
		}

		public override void Deinitialize()
		{
			base.Deinitialize();
			
			m_Button.onClick.AddListener(HandleButtonClick);
		}

		private void HandleButtonClick()
		{
			m_ScreenStack.OpenScreen(m_OptionsScreenPrefab);
		}
	}
}