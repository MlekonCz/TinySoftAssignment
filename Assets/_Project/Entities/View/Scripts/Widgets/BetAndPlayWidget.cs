namespace GUI.Widgets
{
	using System;
	using Core;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	public class BetAndPlayWidget : ScreenWidget
	{
		[SerializeField] private TextMeshProUGUI m_BetText;
		[SerializeField] private Button m_MinusButton;
		[SerializeField] private Button m_PlusButton;
		[SerializeField] private Button m_PlayButton;
		
		private UserData m_UserData;
		private MiniGamesSettings m_Settings;
		private ScreenView m_Owner;

		public long BetValue => m_Settings.BetAmounts[m_UserData.BetIndex];
		public Action SignalPlayClicked { get; set; }

		public override void Initialize(ScreenStack stack, ScreenView owner, ServiceLocator locator)
		{
			base.Initialize(stack, owner, locator);
			
			m_UserData = locator.Get<UserService>().UserData;
			m_Settings = locator.Get<MasterSettings>().MiniGamesSettings;
			m_Owner = owner;
		}

		public override void Deinitialize()
		{
			base.Deinitialize();
		}

		public override void OnShow()
		{
			base.OnShow();

			m_UserData.SignalBalanceChanged += CheckBetValue;
			m_Owner.SignalScreenBusyChanged += HandleScreenBusyChanged;

			m_MinusButton.onClick.AddListener(HandleMinusClicked);
			m_PlusButton.onClick.AddListener(HandlePlusClicked);
			m_PlayButton.onClick.AddListener(HandlePlayClicked);

			CheckBetValue();
		}

		public override void OnHide()
		{
			base.OnHide();
			
			m_UserData.SignalBalanceChanged -= CheckBetValue;
			m_Owner.SignalScreenBusyChanged -= HandleScreenBusyChanged;
			
			m_MinusButton.onClick.RemoveListener(HandleMinusClicked);
			m_PlusButton.onClick.RemoveListener(HandlePlusClicked);
			m_PlayButton.onClick.RemoveListener(HandlePlayClicked);
		}

		public void BlockButtons(bool block)
		{
			if (block)
			{
				m_PlayButton.interactable = false;
				m_MinusButton.interactable = false;
				m_PlusButton.interactable = false;
			}
			else
			{
				CheckBetValue();
			}
		}
		
		private void CheckBetValue()
		{
			// Check bet range!
			var clampBetIndex = Mathf.Clamp(m_UserData.BetIndex, 0, m_Settings.BetAmounts.Count - 1);
			if (clampBetIndex != m_UserData.BetIndex)
			{
				m_UserData.SetBetIndex(clampBetIndex);
			}

			m_BetText.text = BetValue.ToString("N0");
			
			m_PlayButton.interactable = m_UserData.Balance >= BetValue;
			m_MinusButton.interactable = m_UserData.BetIndex > 0;
			m_PlusButton.interactable = m_UserData.BetIndex < m_Settings.BetAmounts.Count - 1;
		}
		
		private void HandleMinusClicked()
		{
			if (m_UserData.BetIndex <= 0)
				return;
			
			m_UserData.SetBetIndex(m_UserData.BetIndex - 1);
			CheckBetValue();
		}

		private void HandlePlusClicked()
		{
			if (m_UserData.BetIndex >= m_Settings.BetAmounts.Count - 1)
				return;
			
			m_UserData.SetBetIndex(m_UserData.BetIndex + 1);
			CheckBetValue();
		}
		
		private void HandlePlayClicked()
		{
			if (m_UserData.Balance < BetValue)
			{
				Debug.Log("Not enough currency to start the game!");
				return;
			}

			if (SignalPlayClicked != null)
			{
				SignalPlayClicked.Invoke();
			}
		}
		
		private void HandleScreenBusyChanged(bool busy)
		{
			BlockButtons(busy);
		}
	}
}