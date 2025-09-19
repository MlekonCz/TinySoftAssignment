namespace GUI.Widgets
{
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	public class WheelSegmentWidget : MonoBehaviour
	{
		[SerializeField] private Image m_SegmentBackground;
		[SerializeField] private TextMeshProUGUI m_Text;
		[SerializeField] private Image m_Image;
		
		public WheelWidget.WheelSegmentConfig Config { get; private set; }

		public void Setup(WheelWidget.WheelSegmentConfig config, float angle, float arcAngle)
		{
			Config = config;
			
			if (m_SegmentBackground != null)
			{
				m_SegmentBackground.fillAmount = arcAngle / 360f;
				m_SegmentBackground.transform.localRotation = Quaternion.Euler(0, 0, arcAngle / 2);
			}

			SetColor(config.Color);
			SetText(config.Text);
			SetSprite(config.Sprite);
			
			transform.localRotation = Quaternion.Euler(0, 0, angle);
		}

		public void SetColor(Color color)
		{
			if (m_SegmentBackground != null)
			{
				m_SegmentBackground.color = color;
			}
		}

		public void SetText(string text)
		{
			if (string.IsNullOrEmpty(text) == false && m_Text != null)
			{
				m_Text.text = text;
			}
		}

		public void SetSprite(Sprite sprite)
		{
			if (sprite != null && m_Image != null)
			{
				m_Image.sprite = sprite;
			}
		}

		public void PlayWinGlowAnim()
		{
			
		}
	}
}