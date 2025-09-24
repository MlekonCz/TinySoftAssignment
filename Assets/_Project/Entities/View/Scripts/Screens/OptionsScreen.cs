using Entities.Core.Scripts;
using Entities.Core.Scripts.GUI;
using Entities.Core.Scripts.User;
using UnityEngine;
using UnityEngine.UI;

namespace Entities.View.Scripts.Screens
{
	public class OptionsScreen : ScreenView
	{
		[SerializeField] private Button m_ResetUserButton;

		private UserService userService;
		internal override void Initialize(ScreenStack stack, ServiceLocator locator)
		{
			base.Initialize(stack, locator);
			userService = locator.Get<UserService>();
			m_ResetUserButton.onClick.AddListener(HandleResetUserClick);
		}

		internal override void Deinitialize()
		{
			base.Deinitialize();
			
			m_ResetUserButton.onClick.RemoveListener(HandleResetUserClick);
		}

		private void HandleResetUserClick()
		{
			userService.ResetUser();
		}
	}
}