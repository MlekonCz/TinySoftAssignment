using UnityEngine;

namespace _Project.Entities.Minigame.PlinkoMinigame.Scripts
{
    public class BallWidget : MonoBehaviour
    {
        [SerializeField] 
        private Transform m_Visual;
        
        public void ApplyVisualBaseline(Vector3 baselineLocalPos, float yOffset)
        {
            if (m_Visual != null) m_Visual.localPosition = baselineLocalPos + Vector3.up * yOffset;
            else transform.localPosition = baselineLocalPos + Vector3.up * yOffset; // fallback
        }
    }
}