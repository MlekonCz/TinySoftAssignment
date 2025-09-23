using UnityEngine;

namespace _Project.Entities.Minigame.PlinkoMinigame.Scripts
{
    public class BallWidget : MonoBehaviour
    {
        [SerializeField] 
        private Transform m_Visual;
        
        public void ApplyVisualBaseline(Vector3 baselineLocalPos, float xOffset, float yOffset)
        {
            m_Visual.localPosition = baselineLocalPos + new Vector3(xOffset, yOffset, 0);
        }
    }
}