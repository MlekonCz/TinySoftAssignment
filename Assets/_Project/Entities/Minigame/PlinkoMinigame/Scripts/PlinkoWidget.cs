using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core;
using Core.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GUI.Widgets;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Entities.Minigame.PlinkoMinigame.Scripts
{
    public class PlinkoWidget: ScreenWidget
    {
        [Serializable]
        public class PlinkoBoxConfig : IWinResult, IWeightRandomItem
        {
            public int WinPercent;
            public Color Color;
            public Sprite Sprite;
            public string Text;
            public float RandomWeight = 1f;

            int IWinResult.WinPercent => WinPercent;
            float IWeightRandomItem.Weight => RandomWeight;
        }

        [Serializable]
        public class PlinkoConfig
        {
            [TableList]
            public List<PlinkoBoxConfig> Box = new();
        }

        
        [BoxGroup("Ball")]
        [SerializeField]
        private Transform _BallParent;
        
        [BoxGroup("Ball")]
        [SerializeField]
        private GameObject m_BallPrefab;

        [BoxGroup("Ball")]
        [SerializeField]
        private float m_RoundDuration = 5f;

        [BoxGroup("Ball")]
        [SerializeField]
        private List<AnimationCurve> m_DropCurves = new();

        [BoxGroup("Ball")]
        [SerializeField]
        private List<AnimationCurve> m_BounceLeftCurves = new();

        [BoxGroup("Ball")]
        [SerializeField]
        private List<AnimationCurve> m_BounceRightCurves = new();

      
        [BoxGroup("Box")]
        [SerializeField]
        private PlinkoBoxWidget m_BoxPrefab;
        
        [BoxGroup("Box")]
        [SerializeField]
        private RectTransform m_BoxParent;

        [BoxGroup("Box")]
        [SerializeField]
        private float _BoxOffset = 20f;
        
        [BoxGroup("Box")]
        [SerializeField]
        private ParticleSystem[] m_ConfettiParticles;

        [BoxGroup("Obstacle")]
        [SerializeField]
        private PlinkoObstacleWidget m_ObstaclePrefab;
   
        [BoxGroup("Obstacle")]
        [SerializeField]
        private RectTransform m_ObstacleParent;
      
        [BoxGroup("Obstacle")]
        [SerializeField]
        private float _ObstacleRowOffset = 50f;

        [BoxGroup("Obstacle")]
        [SerializeField]
        private float _ObstacleHeightOffset = 30f;
        
        [BoxGroup("Obstacle")]
        [SerializeField]
        private float _MinimalObstacleHeightOffset = 50f;
        
        [BoxGroup("Obstacle")]
        [SerializeField]
        private float _MinObstacleSize = 20f;

        
        private float m_boxLayoutStartX;
        private float m_boxLayoutStepX;
        
        private PlinkoObstacleWidget[,] m_ObstacleGrid;
		private List<int> m_indexOrder = new();

        private List<PlinkoBoxWidget> m_boxWidgets { get; } = new();

		public override void Initialize(ScreenStack stack, ScreenView owner, ServiceLocator locator)
		{
			base.Initialize(stack, owner, locator);
			
			// Hide segment prefab
			m_BoxPrefab.gameObject.SetActive(false);
		}

		public void SetupSegments(PlinkoConfig config)
		{
			SetUpBoxes(config);
			SetUpObstacles(config);
		}
		
		private void SetUpBoxes(PlinkoConfig config)
		{
			// Clear old segment setup
			foreach (var segment in m_boxWidgets)
			{
				Destroy(segment.gameObject);
			}
			m_boxWidgets.Clear();
			var boxCount = config.Box.Count;
			if (boxCount  <= 0) return;
			
			
			var offsetsTotal = (boxCount + 1) * _BoxOffset;
			
			var prefabBoxWidth = ((RectTransform)m_BoxPrefab.transform).rect.width;
			var boxParentWidth = m_BoxParent.rect.width;
			var maxBoxWidth = (boxParentWidth - offsetsTotal) / boxCount;
			var boxWidth = Mathf.Min(prefabBoxWidth, maxBoxWidth);
			
			var contentWidth = boxCount * boxWidth + offsetsTotal;
			
			m_boxLayoutStartX = -contentWidth * 0.5f + _BoxOffset + boxWidth * 0.5f;
			m_boxLayoutStepX = boxWidth + _BoxOffset;
			
			
			for (var i = 0; i < boxCount; i++)
			{
				var sConfig = config.Box[i];
				var box = Instantiate(m_BoxPrefab, m_BoxParent);
				box.gameObject.SetActive(true);
				
				((RectTransform)box.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boxWidth);

				var x = m_boxLayoutStartX + i * m_boxLayoutStepX;
				box.transform.localPosition = new Vector3(x, 0f, 0f);

				box.Setup(sConfig);
				m_boxWidgets.Add(box);
			}
		}

		
		private void SetUpObstacles(PlinkoConfig config)
		{
		}

		[Button]
		public async UniTask AnimateRotationToSegment(int targetBoxIndex, CancellationToken ct)
		{
		}
}