namespace GUI.Widgets
{
	using Core;
	using TMPro;
	using UnityEngine;

	public class UserBalanceWidget : ScreenWidget
	{
		[SerializeField] private TextMeshProUGUI m_BalanceText;
		
		private UserData m_UserData;

		public override void Initialize(ScreenStack stack, ScreenView owner, ServiceLocator locator)
		{
			base.Initialize(stack, owner, locator);
			
			m_UserData = locator.Get<UserService>().UserData;
		}
		
		public override void OnShow()
		{
			base.OnShow();

			m_UserData.SignalBalanceChanged += RefreshBalance;
			RefreshBalance();
		}

		public override void OnHide()
		{
			base.OnHide();
			
			m_UserData.SignalBalanceChanged -= RefreshBalance;
		}

		private void RefreshBalance()
		{
			m_BalanceText.text = m_UserData.Balance.ToString("N0");
		}
	}
}