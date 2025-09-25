using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Entities.Core.Scripts;
using Entities.Core.Scripts.GUI;
using Entities.Core.Scripts.Utility;
using Entities.Core.Scripts.Utils;
using Entities.Minigame.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Entities.Minigame.PlinkoMinigame.Scripts
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
        private float m_GameSpeedMultiplier = 1f;

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
        private Ball m_BallPrefab;

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
        private PlinkoBox m_BoxPrefab;

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

        public List<PlinkoBox> BoxWidgets { get; } = new();

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
            foreach (var segment in BoxWidgets) Destroy(segment.gameObject);

            BoxWidgets.Clear();
            var boxCount = config.Box.Count;
            if (boxCount <= 0) return;


            //adjusts the size of boxes based on size of screen
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
            if (m_ObstacleGrid != null)
            { 
                foreach (var seg in m_ObstacleGrid)
                {
                    if (seg) Destroy(seg.gameObject);
                }
            }
            var gridSize = config.Box.Count - 1;
            if (gridSize <= 0) return;

            m_ObstacleGrid = new RectTransform[gridSize, gridSize];

            m_indexOrder = new List<int>(gridSize);
            for (var i = 0; i < gridSize; i++) m_indexOrder.Add(i);

            //adjusts the size of obstacles based on size of screen
            var parentHeight = m_ObstacleParent.rect.height;
            var baseObstacleSize = ((RectTransform)m_ObstaclePrefab.transform).rect.height; // square
            var targetObstacleSize = baseObstacleSize;
            var verticalRowSpacing = m_ObstacleHeightOffset;

            var expectedHeight = gridSize * verticalRowSpacing + targetObstacleSize;
            if (expectedHeight > parentHeight)
            {
                var heightScale  = parentHeight / expectedHeight;
                targetObstacleSize = Mathf.Max(baseObstacleSize * heightScale , m_MinObstacleSize);
                verticalRowSpacing = Mathf.Max(m_ObstacleHeightOffset * heightScale , m_MinimalObstacleHeightOffset);

                var maxRowSpacing = (parentHeight - targetObstacleSize) / gridSize;
                if (maxRowSpacing < verticalRowSpacing) verticalRowSpacing = Mathf.Max(maxRowSpacing, m_MinimalObstacleHeightOffset);

                var maxAvailableObstacleSize = parentHeight - gridSize * verticalRowSpacing;
                if (maxAvailableObstacleSize < targetObstacleSize) targetObstacleSize = Mathf.Max(maxAvailableObstacleSize, m_MinObstacleSize);
            }

            var gapStartX = m_boxLayoutStartX + m_boxLayoutStepX * 0.5f;
          
            var maxGapX = gapStartX + (gridSize - 1) * m_boxLayoutStepX;

            
            for (var row = 0; row < gridSize; row++)
            {
                var countInRow = row + 1;

                var rowIndexFromBottom = gridSize - 1 - row;
                var yLocal = m_ObstacleParent.rect.yMin + verticalRowSpacing + rowIndexFromBottom * verticalRowSpacing;

                var leftGapIndex = Mathf.Clamp((gridSize - countInRow) / 2, 0, Mathf.Max(0, gridSize - countInRow));
                var baseX = gapStartX + leftGapIndex * m_boxLayoutStepX;

                var isShiftedRow = gridSize % 2 == 0 ? row % 2 == 0 : row % 2 != 0;
                var horizontalShift = isShiftedRow ? m_boxLayoutStepX * 0.5f : 0f;

                var firstXWithShift = baseX + horizontalShift;
                var lastXWithShift = baseX + horizontalShift + (countInRow - 1) * m_boxLayoutStepX;
                if (firstXWithShift < gapStartX) horizontalShift += gapStartX - firstXWithShift;
                if (lastXWithShift > maxGapX) horizontalShift -= lastXWithShift - maxGapX;

                for (var j = 0; j < countInRow; j++)
                {
                    var obstacle = Instantiate(m_ObstaclePrefab, m_ObstacleParent);
                    obstacle.gameObject.SetActive(true);

                    m_ObstacleGrid[row, j] = obstacle;

                    var xLocal = baseX + horizontalShift + j * m_boxLayoutStepX;
                    xLocal = Mathf.Clamp(xLocal, gapStartX, maxGapX);

                    var t = obstacle.transform;
                    t.localPosition = new Vector3(xLocal, yLocal, 0f);
                    t.localRotation = Quaternion.identity;

                    var rt = (RectTransform)t;
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetObstacleSize);
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetObstacleSize);
                }
            }
        }

        private void SetUpBallSpawningImage()
        {
            for (int i = 0; i < m_ObstacleGrid.GetLength(0); i++)
            {
                if (m_ObstacleGrid[0,i] != null)
                {
                    var obstacle = m_ObstacleGrid[0,i];
                    var ballSpawningPosition = obstacle.position + Vector3.up * (m_AboveObstacleSpawn +  obstacle.ToScreenSpaceRect().height / 2);
                    m_BallSpawningImage.transform.position = ballSpawningPosition;
                    break;
                }
            }
            
        }

        [Button]
        public async UniTask AnimateRotationToSegment(int targetBoxIndex, CancellationToken cancellationToken)
        {
            if (m_BallPrefab == null || m_BallParent == null)
                return;

            var rows = m_ObstacleGrid?.GetLength(0) ?? 0;
            var cols = m_ObstacleGrid?.GetLength(1) ?? 0;
            if (rows == 0 || cols == 0 || m_ObstacleGrid == null)
                return;

            if (BoxWidgets == null || targetBoxIndex < 0 || targetBoxIndex >= BoxWidgets.Count)
                return;

            //selects path to the target box
            var pathLength = rows;
            var path = new List<int>(pathLength);
            var choices = new List<bool>(pathLength);

            if (pathLength > 0)
            {
                var currentSegmentIndex = -1;
                var randomPath = FindPaths(targetBoxIndex).SelectRandom();

                path = randomPath.Path;

                for (var i = 0; i < path.Count; i++)
                {
                    var nextSegmentIndex = path[i];
                    if (currentSegmentIndex >= 0) choices.Add(nextSegmentIndex > currentSegmentIndex);
                    currentSegmentIndex = nextSegmentIndex;
                }
                choices.Add(targetBoxIndex >= currentSegmentIndex + 1);
            }

            //spawns new ball
            var topObstacle = m_ObstacleGrid[0, path[0]];
            if (topObstacle == null) return;
            var startAbove = topObstacle.position + Vector3.up * (m_AboveObstacleSpawn +  topObstacle.ToScreenSpaceRect().height / 2);

            var ball = Instantiate(m_BallPrefab, m_BallParent);
            ball.gameObject.SetActive(true);

            var ballT = ball.transform;
            ballT.position = startAbove;
            
            try
            {
                //physics logic for ball
                var winningBox = BoxWidgets[targetBoxIndex];
                for (var i = 0; i < rows + 1; i++)
                {
                    var initialXPos = ball.transform.position.x;

                    var goRight = choices[Mathf.Min(choices.Count-1, i)];
                    var targetPos = GetTargetObject(i, goRight, out var angle);
                
                    var timeToPeak = Mathf.Abs(-ball.YVelocity / m_BallAcceleration);
                    var peakY = ball.transform.position.y + ball.YVelocity * timeToPeak + 0.5f * m_BallAcceleration * timeToPeak * timeToPeak;
              
                    var timeToReachY = Mathf.Sqrt((2 * (peakY - targetPos.y)) / m_BallAcceleration);

                    var totalTimeToReachTarget = timeToPeak + timeToReachY;
                
                    var elapsedTime = 0f;
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        elapsedTime += Time.deltaTime * m_GameSpeedMultiplier;
                    
                        var pos = ball.transform.position;
                        pos.y += ball.YVelocity * Time.deltaTime * m_GameSpeedMultiplier;
                    
                        ball.YVelocity -= m_BallAcceleration * Time.deltaTime * m_GameSpeedMultiplier;

                        var t = Mathf.Clamp01(elapsedTime / totalTimeToReachTarget);
                        pos.x = Mathf.Lerp(initialXPos, targetPos.x, t);

                        ball.transform.position = pos;

                        if (ball.transform.position.y <= targetPos.y)
                        {
                            var ang = Mathf.Abs(angle);
                            var angleFactor = Mathf.Cos(ang * Mathf.Deg2Rad);

                            var preserved = m_BallBouncePreservedMomentum * angleFactor;

                            ball.YVelocity *= -preserved;
                            break;
                        }

                        await UniTask.Yield(cancellationToken);
                    }
                }
                winningBox.PlayWinAnim();
            }
            finally
            {
                if (ball != null)
                    Destroy(ball.gameObject);
            }

            return;

            Vector2 GetTargetObject(int index, bool goRight, out float angle)
            {
                if (index > path.Count - 1)
                {
                    angle = 0;
                    return BoxWidgets[targetBoxIndex].transform.position;
                }
                var obstacle = m_ObstacleGrid[index, path[index]];

                var obstacleR = obstacle.ToScreenSpaceRect().height / 2;
                var balR = ((RectTransform) ball.transform).ToScreenSpaceRect().width / 2;

                var vec = (obstacleR + balR) * Vector3.up;
                var sign = (goRight ? -1 : 1);
                angle = Random.Range(m_MinBounceAngle , m_MaxBounceAngle);
                var rotatedVec = Quaternion.Euler(0, 0, angle * sign) * vec;
                return rotatedVec + obstacle.transform.position;
            }
        }

        private List<BallPath> FindPaths(int targetBoxIndex)
        {
            var paths = new List<BallPath>();

            var rowCount = m_ObstacleGrid.GetLength(0);
            if (rowCount <= 0) return paths;

            var colCount = rowCount;
            var boxCount = rowCount + 1;
            if (targetBoxIndex < 0 || targetBoxIndex >= boxCount) return paths;

            var startColA = Mathf.Clamp(targetBoxIndex - 1, 0, colCount - 1);
            var startColB = Mathf.Clamp(targetBoxIndex,0, colCount - 1);

            var currentPath = new int[rowCount];
            var visited = new HashSet<int>(2);

            void Explore(int row, int col)
            {
                if (m_ObstacleGrid[row, col] == null) return;

                currentPath[row] = col;

                if (row == 0)
                {
                    paths.Add(new BallPath { Path = new List<int>(currentPath) });
                    return;
                }

                Explore(row - 1, col);
                if (col - 1 >= 0) Explore(row - 1, col - 1);
            }

            if (visited.Add(startColA)) Explore(rowCount - 1, startColA);
            if (visited.Add(startColB)) Explore(rowCount - 1, startColB);

            return paths;
        }

        private class BallPath
        {
            public List<int> Path = new();
        }
    }
}