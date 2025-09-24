using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core;
using Core.Utility;
using Core.Utils;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _Project.Entities.Minigame.PlinkoMinigame.Scripts
{
    public class PlinkoWidget : ScreenWidget
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
        private Transform m_BallSpawningImage;
        
        [BoxGroup("Ball")]
        [SerializeField]
        private float m_BallAcceleration = 9.81f;
        
        [BoxGroup("Ball")]
        [SerializeField]
        private float m_MaxBounceAngle = 35;
        
        [BoxGroup("Ball")]
        [SerializeField]
        private float m_MinBounceAngle = 25;

        [BoxGroup("Ball")]
        [Range(0f, 1f)]
        [SerializeField]
        private float m_BallBouncePreservedMomentum = 0.5f;

        [BoxGroup("Ball")]
        [SerializeField]
        private Transform m_BallParent;

        [BoxGroup("Ball")]
        [SerializeField]
        private BallWidget m_BallPrefab;

        [BoxGroup("Ball")]
        [SerializeField]
        private float m_RoundDuration = 5f;

        [BoxGroup("Ball")]
        [Range(0f, 2f)]
        [SerializeField]
        private float m_DurationRationSideDropToTopDrop = 1f;

        [BoxGroup("Ball")]
        [SerializeField]
        private float m_AboveObstacleSpawn = 1f;

        [BoxGroup("Box")]
        [SerializeField]
        private PlinkoBoxWidget m_BoxPrefab;

        [BoxGroup("Box")]
        [SerializeField]
        private RectTransform m_BoxParent;

        [FormerlySerializedAs("_BoxOffset")]
        [BoxGroup("Box")]
        [SerializeField]
        private float m_BoxOffset = 20f;
        
        [BoxGroup("Obstacle")]
        [SerializeField]
        private RectTransform m_ObstaclePrefab;

        [BoxGroup("Obstacle")]
        [SerializeField]
        private RectTransform m_ObstacleParent;

        [FormerlySerializedAs("_ObstacleRowOffset")]
        [BoxGroup("Obstacle")]
        [SerializeField]
        private float m_ObstacleRowOffset = 50f;

        [FormerlySerializedAs("_ObstacleHeightOffset")]
        [BoxGroup("Obstacle")]
        [SerializeField]
        private float m_ObstacleHeightOffset = 30f;

        [FormerlySerializedAs("_MinimalObstacleHeightOffset")]
        [BoxGroup("Obstacle")]
        [SerializeField]
        private float m_MinimalObstacleHeightOffset = 50f;

        [FormerlySerializedAs("_MinObstacleSize")]
        [BoxGroup("Obstacle")]
        [SerializeField]
        private float m_MinObstacleSize = 20f;


        private float m_boxLayoutStartX;
        private float m_boxLayoutStepX;

        private RectTransform[,] m_ObstacleGrid;
        private List<int> m_indexOrder = new();

        public List<PlinkoBoxWidget> BoxWidgets { get; } = new();

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
            SetUpBallSpawningImage();
        }

        private void SetUpBoxes(PlinkoConfig config)
        {
            // Clear old segment setup
            foreach (var segment in BoxWidgets) Destroy(segment.gameObject);

            BoxWidgets.Clear();
            var boxCount = config.Box.Count;
            if (boxCount <= 0) return;


            var offsetsTotal = (boxCount + 1) * m_BoxOffset;

            var prefabBoxWidth = ((RectTransform)m_BoxPrefab.transform).rect.width;
            var boxParentWidth = m_BoxParent.rect.width;
            var maxBoxWidth = (boxParentWidth - offsetsTotal) / boxCount;
            var boxWidth = Mathf.Min(prefabBoxWidth, maxBoxWidth);

            var contentWidth = boxCount * boxWidth + offsetsTotal;

            m_boxLayoutStartX = -contentWidth * 0.5f + m_BoxOffset + boxWidth * 0.5f;
            m_boxLayoutStepX = boxWidth + m_BoxOffset;


            for (var i = 0; i < boxCount; i++)
            {
                var sConfig = config.Box[i];
                var box = Instantiate(m_BoxPrefab, m_BoxParent);
                box.gameObject.SetActive(true);

                ((RectTransform)box.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boxWidth);

                var x = m_boxLayoutStartX + i * m_boxLayoutStepX;
                box.transform.localPosition = new Vector3(x, 0f, 0f);

                box.Setup(sConfig);
                BoxWidgets.Add(box);
            }
        }


        private void SetUpObstacles(PlinkoConfig config)
        {
            // Clear old
            if (m_ObstacleGrid != null)
            { 
                foreach (var seg in m_ObstacleGrid)
                {
                    if (seg) Destroy(seg.gameObject);
                }
            }

            var gridSize = config.Box.Count - 1;
            m_ObstacleGrid = new RectTransform[gridSize, gridSize];

            m_indexOrder = new List<int>(gridSize);
            for (var i = 0; i < gridSize; i++) m_indexOrder.Add(i);

            var parentHeight = m_ObstacleParent.rect.height;
            var basePegSize = ((RectTransform)m_ObstaclePrefab.transform).rect.height; // square
            var targetPegSize = basePegSize;
            var verticalRowSpacing = m_ObstacleHeightOffset;

            var expectedHeight = gridSize * verticalRowSpacing + targetPegSize;
            if (expectedHeight > parentHeight)
            {
                var s = parentHeight / expectedHeight;
                targetPegSize = Mathf.Max(basePegSize * s, m_MinObstacleSize);
                verticalRowSpacing = Mathf.Max(m_ObstacleHeightOffset * s, m_MinimalObstacleHeightOffset);

                var maxRowDyGivenPeg = (parentHeight - targetPegSize) / gridSize;
                if (maxRowDyGivenPeg < verticalRowSpacing)
                    verticalRowSpacing = Mathf.Max(maxRowDyGivenPeg, m_MinimalObstacleHeightOffset);

                var maxPegGivenRowDy = parentHeight - gridSize * verticalRowSpacing;
                if (maxPegGivenRowDy < targetPegSize)
                    targetPegSize = Mathf.Max(maxPegGivenRowDy, m_MinObstacleSize);
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

                var leftGapIndex = Mathf.Clamp((totalGaps - countInRow) / 2, 0, Mathf.Max(0, totalGaps - countInRow));
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

                    var col = j;
                    m_ObstacleGrid[row, col] = obstacle;

                    var xLocal = baseX + desiredShift + j * gapStepX;
                    xLocal = Mathf.Clamp(xLocal, minGapX, maxGapX);

                    var t = obstacle.transform;
                    t.localPosition = new Vector3(xLocal, yLocal, 0f);
                    t.localRotation = Quaternion.identity;

                    var rt = (RectTransform)t;
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetPegSize);
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetPegSize);
                }
            }
        }

        private void SetUpBallSpawningImage()
        {
            var topObstacle =m_ObstacleGrid[0,FindValidObstacles(0).First()];
            var ballSpawningPosition = topObstacle.position + Vector3.up * (m_AboveObstacleSpawn +  topObstacle.ToScreenSpaceRect().height / 2);
            
            m_BallSpawningImage.transform.position = ballSpawningPosition;
        }

        [Button]
        public async UniTask AnimateRotationToSegment(int targetBoxIndex, CancellationToken ct)
        {
            if (m_BallPrefab == null || m_BallParent == null)
                return;

            var rows = m_ObstacleGrid?.GetLength(0) ?? 0;
            var cols = m_ObstacleGrid?.GetLength(1) ?? 0;
            if (rows == 0 || cols == 0 || m_ObstacleGrid == null)
                return;

            if (BoxWidgets == null || targetBoxIndex < 0 || targetBoxIndex >= BoxWidgets.Count)
                return;

            AnimationCurve Pick(IReadOnlyList<AnimationCurve> list, AnimationCurve fallback = null)
            {
                if (list != null && list.Count > 0)
                    return list[Random.Range(0, list.Count)];
                return fallback ?? AnimationCurve.Linear(0, 0, 1, 1);
            }

            var pathLength = rows;
            var path = new List<int>(pathLength);
            var choices = new List<bool>(pathLength);

            bool CanStillReachBox(int colIdxAtRow, int row)
            {
                var remRows = pathLength - (row + 1);
                var j = ColToRowSlotJ(row, colIdxAtRow);
                if (j < 0) return false; // safety

                var minBox = j;
                var maxBox = j + remRows + 1;

                minBox = Mathf.Max(minBox, 0);
                maxBox = Mathf.Min(maxBox, BoxWidgets.Count - 1);

                return targetBoxIndex >= minBox && targetBoxIndex <= maxBox;
            }

            if (pathLength > 0)
            {
                var currentSegmentIndex = -1;
                for (var i = 0; i < pathLength; i++)
                {
                    var validObstacles = FindValidObstacles(i, currentSegmentIndex);

                    var feasible = validObstacles.Where(idx => CanStillReachBox(idx, i)).ToList();

                    var candidatePool = feasible.Count > 0 ? feasible : validObstacles;

                    var nextObstacleIndex = candidatePool.SelectRandom();

                    if (currentSegmentIndex >= 0) choices.Add(nextObstacleIndex > currentSegmentIndex);

                    path.Add(nextObstacleIndex);
                    currentSegmentIndex = nextObstacleIndex;
                }

                choices.Add(targetBoxIndex >= currentSegmentIndex + 1);
            }

            var topObstacle = m_ObstacleGrid[0, path[0]];
            var startAbove = topObstacle.position + Vector3.up * (m_AboveObstacleSpawn +  topObstacle.ToScreenSpaceRect().height / 2);

            var ball = Instantiate(m_BallPrefab, m_BallParent);
            ball.gameObject.SetActive(true);

            var ballT = ball.transform;
            ballT.position = startAbove;


            var winningBox = BoxWidgets[targetBoxIndex];


            for (var i = 0; i < rows + 1; i++)
            {
                var initialXPos = ball.transform.position.x;

                var goRight = choices[Mathf.Min(choices.Count-1, i)];
                var targetPos = GetTargetObject(i, goRight);
                
                var timeToPeak = Mathf.Abs(-ball.YVelocity / m_BallAcceleration);
                var peakY = ball.transform.position.y + ball.YVelocity * timeToPeak + 0.5f * m_BallAcceleration * timeToPeak * timeToPeak;
              
                var timeToReachY = Mathf.Sqrt((2 * (peakY - targetPos.y)) / m_BallAcceleration);

                var totalTimeToReachTarget = timeToPeak + timeToReachY;
                
                var elapsedTime = 0f;
                while (true)
                {
                    elapsedTime += Time.deltaTime;
                    
                    var pos = ball.transform.position;
                    pos.y += ball.YVelocity * Time.deltaTime;
                    
                    ball.YVelocity -= m_BallAcceleration * Time.deltaTime;

                    var t = Mathf.Clamp01(elapsedTime / totalTimeToReachTarget);
                    pos.x = Mathf.Lerp(initialXPos, targetPos.x, t);

                    ball.transform.position = pos;

                    if (ball.transform.position.y <= targetPos.y)
                    {
                        ball.YVelocity *= -m_BallBouncePreservedMomentum;
                        break;
                    }

                    await UniTask.Yield();
                }
            }
            
            winningBox.PlayWinAnim();
            Destroy(ball);
            return;
            
            
            Vector2 GetTargetObject(int index, bool goRight)
            {
                if (index > path.Count - 1)
                {
                    return BoxWidgets[targetBoxIndex].transform.position;

                }
                var obstacle = m_ObstacleGrid[index, path[index]];

                var obstacleR = obstacle.ToScreenSpaceRect().height / 2;
                var balR = ((RectTransform) ball.transform).ToScreenSpaceRect().width / 2;

                var vec = (obstacleR + balR) * Vector3.up;
                var sign = (goRight ? -1 : 1);
                var rotatedVec = Quaternion.Euler(0, 0, Random.Range(m_MinBounceAngle , m_MaxBounceAngle) * sign) * vec;
                return rotatedVec + obstacle.transform.position;
            }
        }

        private List<int> FindValidObstacles(int row, int currentObstacleIndex = -1)
        {
            var result = new List<int>();
            var rows = m_ObstacleGrid.GetLength(0);
            var cols = m_ObstacleGrid.GetLength(1);
            if ((uint)row >= (uint)rows) return result;

            //when below 0, looks for first obstacle on top of pyramid
            if (currentObstacleIndex < 0)
            {
                var col0 = m_indexOrder != null && m_indexOrder.Count > 0 ? m_indexOrder[0] : -1;
                if (col0 >= 0 && col0 < cols && m_ObstacleGrid[row, col0] != null)
                {
                    result.Add(col0);
                    return result;
                }

                for (var c = 0; c < cols; c++)
                    if (m_ObstacleGrid[row, c] != null)
                    {
                        result.Add(c);
                        break;
                    }

                return result;
            }
            

            if (currentObstacleIndex < cols && m_ObstacleGrid[row, currentObstacleIndex] != null)
                result.Add(currentObstacleIndex);

            var nextObstacleIndex = currentObstacleIndex + 1;
            if (nextObstacleIndex >= 0 && nextObstacleIndex < cols && m_ObstacleGrid[row, nextObstacleIndex] != null)
                result.Add(nextObstacleIndex);

            if (result.Count > 1 && result[0] > result[1])
                (result[0], result[1]) = (result[1], result[0]);

            return result;
        }

        private int ColToRowSlotJ(int row, int colIndex)
        {
            var j = m_indexOrder.IndexOf(colIndex);
            return j >= 0 && j <= row ? j : -1;
        }
    }
}