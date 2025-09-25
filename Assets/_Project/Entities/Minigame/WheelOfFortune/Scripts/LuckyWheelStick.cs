using UnityEngine;

namespace Entities.Minigame.WheelOfFortune.Scripts
{
    public class LuckyWheelStick : MonoBehaviour
    {
        [SerializeField]
        private RectTransform m_Visual;

        public RectTransform Visual => m_Visual;
    }
}