using Entities.Core.Scripts;
using Entities.Core.Scripts.GUI;
using UnityEngine;
using UnityEngine.UI;

namespace Entities.View.Scripts.Widgets
{
	[RequireComponent(typeof(Button))]
	public class CloseScreenWidget : ScreenWidget
	{
		private Button m_CloseButton;
		
		public override void Initialize(ScreenStack stack, ScreenView owner, ServiceLocator locator)
		{
			base.Initialize(stack, owner, locator);
			
			m_CloseButton = GetComponent<Button>();
			m_CloseButton.onClick.AddListener(OnCloseButtonClicked);
			m_OwnerScreen.SignalScreenBusyChanged += HandleScreenBusyChanged;
		}

		public override void Deinitialize()
		{
			base.Deinitialize();
			
			m_CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
			m_OwnerScreen.SignalScreenBusyChanged -= HandleScreenBusyChanged;
		}

		private void HandleScreenBusyChanged(bool busy)
		{
			m_CloseButton.interactable = busy == false;
		}

		private void OnCloseButtonClicked()
		{
			m_ScreenStack.CloseScreen(m_OwnerScreen);
		}
	}
}