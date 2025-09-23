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
			if (m_ObstacleGrid != null)
			{
				foreach (var seg in m_ObstacleGrid)
				{
					if (seg) Destroy(seg.gameObject);
				}
			}

			var gridSize = config.Box.Count - 1;
			m_ObstacleGrid = new PlinkoObstacleWidget[gridSize, gridSize];

			m_indexOrder = new List<int>(gridSize);
			var start = (gridSize - 1) / 2;
			m_indexOrder.Add(start);
			var firstDir = gridSize % 2 == 0 ? +1 : -1;
			for (var offset = 1; m_indexOrder.Count < gridSize; offset++)
			{
				var f = start + firstDir * offset;
				if (f >= 0 && f < gridSize) m_indexOrder.Add(f);
				if (m_indexOrder.Count >= gridSize) break;

				var b = start - firstDir * offset;
				if (b >= 0 && b < gridSize) m_indexOrder.Add(b);
			}

			var parentHeight = m_ObstacleParent.rect.height;

			var basePegSize = ((RectTransform)m_ObstaclePrefab.transform).rect.height; 
			var targetPegSize = basePegSize;
			var verticalRowSpacing = _ObstacleHeightOffset;

			var expectedHeight = gridSize * verticalRowSpacing + targetPegSize;
			if (expectedHeight > parentHeight)
			{
				var s = parentHeight / expectedHeight; 
				targetPegSize = Mathf.Max(basePegSize * s, _MinObstacleSize);
				verticalRowSpacing = Mathf.Max(_ObstacleHeightOffset * s, _MinimalObstacleHeightOffset);

				var maxRowDyGivenPeg = (parentHeight - targetPegSize) / gridSize;
				if (maxRowDyGivenPeg < verticalRowSpacing)
					verticalRowSpacing = Mathf.Max(maxRowDyGivenPeg, _MinimalObstacleHeightOffset);

				var maxPegGivenRowDy = parentHeight - gridSize * verticalRowSpacing;
				if (maxPegGivenRowDy < targetPegSize)
					targetPegSize = Mathf.Max(maxPegGivenRowDy, _MinObstacleSize);
			}

			var parentBottomLocalY = m_ObstacleParent != null
				? m_ObstacleParent.rect.yMin
				: -verticalRowSpacing * 0.5f;

			var gapStartX = m_boxLayoutStartX + m_boxLayoutStepX * 0.5f;
			var gapStepX = m_boxLayoutStepX;
			var totalGaps = config.Box.Count - 1;

			var minGapX = gapStartX;
			var maxGapX = gapStartX + (totalGaps - 1) * gapStepX;

			for (var row = 0; row < gridSize; row++)
			{
				var countInRow = row + 1;

				var rowFromBottom = gridSize - 1 - row;
				var yLocal = parentBottomLocalY + verticalRowSpacing + rowFromBottom * verticalRowSpacing;

				var leftGapIndex = (totalGaps - countInRow) / 2;
				leftGapIndex = Mathf.Clamp(leftGapIndex, 0, Mathf.Max(0, totalGaps - countInRow));
				var baseX = gapStartX + leftGapIndex * gapStepX;

				var shiftThisRow = totalGaps % 2 == 0 ? row % 2 == 0 : row % 2 != 0;
				var desiredShift = shiftThisRow ? gapStepX * 0.5f : 0f;

				var firstXWithShift = baseX + desiredShift;
				var lastXWithShift = baseX + desiredShift + (countInRow - 1) * gapStepX;
				if (firstXWithShift < minGapX) desiredShift += minGapX - firstXWithShift;
				if (lastXWithShift > maxGapX) desiredShift -= lastXWithShift - maxGapX;

				for (var j = 0; j < countInRow; j++)
				{
					var obstacle = Instantiate(m_ObstaclePrefab, m_ObstacleParent);
					obstacle.gameObject.SetActive(true);

					m_ObstacleGrid[row, m_indexOrder[j]] = obstacle;

					var xLocal = baseX + desiredShift + j * gapStepX;

					var t = obstacle.transform;
					t.localPosition = new Vector3(xLocal, yLocal, 0f);
					t.localRotation = Quaternion.identity;

					var rt = (RectTransform)t;
					rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetPegSize);
					rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetPegSize);
				}
			}
		}

		[Button]
		public async UniTask AnimateRotationToSegment(int targetBoxIndex, CancellationToken ct)
		{
		}
}