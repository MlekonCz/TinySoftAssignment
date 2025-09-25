using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Entities.Minigame.PlinkoMinigame.Scripts
{
    public class PlinkoBox : MonoBehaviour
    { 
        [SerializeField]
        private ParticleSystem[] m_ConfettiParticles;

        [SerializeField] private Image m_SegmentBackground;
        [SerializeField] private TextMeshProUGUI m_Text;
        [SerializeField] private Image m_Image;
		
        public PlinkoWidget.PlinkoBoxConfig Config { get; private set; }

        public void Setup(PlinkoWidget.PlinkoBoxConfig config)
        {
            Config = config;
			
            SetColor(config.Color);
            SetText(config.Text);
            SetSprite(config.Sprite);
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

        public void PlayWinAnim()
        {
            foreach (var particle in m_ConfettiParticles)
            {
                particle.gameObject.SetActive(true);
                particle.Stop();
                particle.Play();
            }

        }
    }
}