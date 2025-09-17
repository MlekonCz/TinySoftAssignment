namespace GUI.Widgets
{
	using System;
	using Core;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	public class GameThumbnail : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI m_NameText;
		[SerializeField] private Button m_Button;

		private MiniGameSettingsBase m_MiniGameSettingsBase;
		private Action<MiniGameSettingsBase> m_ClickAction;

		private void Awake()
		{
			m_Button.onClick.AddListener(GameClicked);
		}

		public void Setup(MiniGameSettingsBase miniGameSettingsBase, Action<MiniGameSettingsBase> onClick)
		{
			m_MiniGameSettingsBase = miniGameSettingsBase;
			m_ClickAction = onClick;
			m_NameText.text = miniGameSettingsBase.GameName;
		}

		private void GameClicked()
		{
			m_ClickAction.Invoke(m_MiniGameSettingsBase);
		}
	}
}