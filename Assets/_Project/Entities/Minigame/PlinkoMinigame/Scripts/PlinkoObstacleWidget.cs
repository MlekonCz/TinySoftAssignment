using TMPro;
using UnityEngine;

namespace _Project.Entities.Minigame.PlinkoMinigame.Scripts
{
    public class PlinkoObstacleWidget : MonoBehaviour
    {
        [SerializeField]
        private Transform m_TopPoint;
        [SerializeField]
        private Transform m_LeftPoint;
        [SerializeField]
        private Transform m_RightPoint;
        [SerializeField]
        public TMP_Text Text;

        
        public Transform TopPoint => m_TopPoint;
        public Transform LeftPoint => m_LeftPoint;
        public Transform RightPoint => m_RightPoint;
    }
}