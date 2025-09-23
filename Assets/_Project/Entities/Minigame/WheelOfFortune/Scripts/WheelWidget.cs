using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core;
using Core.Utility;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GUI.Widgets
{
    public class WheelWidget : ScreenWidget
    {
        [Serializable]
        public class WheelSegmentConfig : IWinResult, IWeightRandomItem
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
        public class WheelConfig
        {
            [TableList]
            public List<WheelSegmentConfig> Segments;
        }

        [BoxGroup("Spinning")]
        [SerializeField]
        private float m_WheelSpinningDuration = 5f;

        [BoxGroup("Spinning")]
        [SerializeField]
        private Transform[] m_WheelSpinners;
        
        [BoxGroup("Spinning")]
        [SerializeField]
        private List<AnimationCurve> m_RotationsCurves = new();

        [BoxGroup("Spinning")]
        [SerializeField]
        private int[] m_NumberOfRotations = { 3, 4, 5 };

        [BoxGroup("Spinning")]
        [SerializeField]
        private Transform m_RotationTransform;

        [BoxGroup("Head")]
        [SerializeField]
        private Transform m_WheelHead;

        [BoxGroup("Head")]
        [SerializeField]
        private AnimationCurve m_BumpCurve;
        
        [BoxGroup("Head")]
        [SerializeField]
        private float m_MaxFallDownAngleDeltaPerSecond = 90;
  
        [BoxGroup("Head")]
        [Range(0f,1f)]
        [SerializeField]
        private float _PctForBumpSound = 0.55f;

        [BoxGroup("Segments")]
        [SerializeField]
        private WheelSegmentWidget m_SegmentPrefab;

        [BoxGroup("Segments")]
        [SerializeField]
        private Transform m_SegmentsParent;
       
        [BoxGroup("Segments")]
        [SerializeField]
        private GameObject _stickPrefab;

        [BoxGroup("Segments")]
        [SerializeField]
        private Transform _sticksParent;
      
        [SerializeField]
        private List<ParticleSystem> m_ConfettiParticles = new();

        private float m_ArcAngle;
        private float m_OneSegmentHalfAngle;
        private float m_AngleOffset;
        private float m_TotalAngleOffset;

        private const float RANDOM_OFFSET_FACTOR = 0.35f;
        
        public List<WheelSegmentWidget> WheelSegments { get; } = new();

		public override void Initialize(ScreenStack stack, ScreenView owner, ServiceLocator locator)
		{
			base.Initialize(stack, owner, locator);
			
			// Hide segment prefab
			m_SegmentPrefab.gameObject.SetActive(false);
		}

		public void SetupSegments(WheelConfig config)
		{
			// Clear old segment setup
			foreach (var segment in WheelSegments)
			{
				Destroy(segment.gameObject);
			}
			if (_sticksParent != null)
			{
				foreach (Transform child in _sticksParent)
				{
					Destroy(child.gameObject);
				}
			}
			WheelSegments.Clear();

			// Setup new config
			m_ArcAngle = 360f / config.Segments.Count;
			for (int i = 0; i < config.Segments.Count; i++)
			{
				var sConfig = config.Segments[i];
				var segment = Instantiate(m_SegmentPrefab, m_SegmentsParent);
				segment.Setup(sConfig, m_ArcAngle * i, m_ArcAngle);
				segment.gameObject.SetActive(true);
				WheelSegments.Add(segment);
				
				if (_stickPrefab != null && _sticksParent != null)
				{
					var stickInstace = Instantiate(_stickPrefab, _sticksParent);
					stickInstace.transform.localRotation = segment.transform.localRotation;
				}
			}
			
		}

		[Button]
		public void SetWheelRotationAngle(float angle)
		{
			m_RotationTransform.localRotation = Quaternion.Euler(0, 0, angle);
		}

		public float GetCurrentWheelRotationAngle()
		{
			return m_RotationTransform.localRotation.eulerAngles.z;
		}

		[Button]
		public void SetRotationToSegment(int index)
		{
			SetWheelRotationAngle(GetSegmentAngle(index));
		}

		[Button]
		public async UniTask AnimateRotationToSegment(int index, CancellationToken cancellationToken)
		{
			if (index < 0 || index >= WheelSegments.Count)
				return;

			var oneSegmentHalfAngle = m_ArcAngle / 2f;

			var fullRotations = m_NumberOfRotations[Random.Range(0, m_NumberOfRotations.Length)] * 360;

			var startAngle = GetCurrentWheelRotationAngle();

			var clampedAngle = Mathf.FloorToInt(oneSegmentHalfAngle * RANDOM_OFFSET_FACTOR);
			var randomOffset = Random.Range(-clampedAngle, clampedAngle + 1);

			var angleToTarget = GetSegmentAngle(index) - startAngle;
			if (angleToTarget < 0) angleToTarget += 360;

			var rotationCurve = m_RotationsCurves[Random.Range(0, m_RotationsCurves.Count)];
			var lastFrameZ = m_WheelHead != null ? m_WheelHead.rotation.eulerAngles.z : 0f;
			var lastSfxPlayedOnSegmentIndex = -1;

			var totalDelta = fullRotations + angleToTarget + randomOffset;

			var timer = 0f;
			while (timer < m_WheelSpinningDuration && !cancellationToken.IsCancellationRequested)
			{
				timer += Time.deltaTime;

				var t = timer / m_WheelSpinningDuration;
				var curveValue = rotationCurve.Evaluate(t);

				var newZ = startAngle + totalDelta * curveValue;
				ApplyWheelRotation(newZ);

				if (m_WheelHead != null) UpdateWheelHeadAnimation(newZ, ref lastFrameZ, ref lastSfxPlayedOnSegmentIndex);

				await UniTask.Yield(cancellationToken);
			}

			SetWheelRotationAngle(GetSegmentAngle(index) + randomOffset);

			if (m_WheelHead != null)
				m_WheelHead.rotation = Quaternion.Euler(0f, 0f, 0f);

			foreach (var particle in m_ConfettiParticles)
			{
				particle.gameObject.SetActive(true);
				particle.Stop();
				particle.Play();
			}

			WheelSegments[index].PlayWinGlowAnim();

			//todo play sound via sound manager that player won/lost
		}
		
		private void UpdateWheelHeadAnimation(float newZ, ref float lastFrameZ, ref int lastSfxIndex)
		{
			var absoluteZ = Mathf.Abs(newZ);
			var currentSegmentIndex = Mathf.CeilToInt(absoluteZ / m_ArcAngle);
			var positionInSegment = (absoluteZ % m_ArcAngle) / m_ArcAngle;
    
			var bumpValue = m_BumpCurve.Evaluate(positionInSegment);
			var smoothedAngle = Mathf.Min(bumpValue,
				Mathf.MoveTowards(lastFrameZ, 0, m_MaxFallDownAngleDeltaPerSecond * Time.deltaTime));
    
			m_WheelHead.rotation = Quaternion.Euler(0, 0, smoothedAngle);
			lastFrameZ = smoothedAngle;

			if (currentSegmentIndex != lastSfxIndex && positionInSegment > _PctForBumpSound)
			{
				// TODO: Play bump sound via sound manager
				lastSfxIndex = currentSegmentIndex;
			}
		}
		
		private void ApplyWheelRotation(float angle)
		{
			SetWheelRotationAngle(angle);
    
			if (m_WheelSpinners == null) return;
    
			var rotation = Quaternion.Euler(0, 0, angle);
			foreach (var spinner in m_WheelSpinners.Where(s => s != null))
			{
				spinner.rotation = rotation;
			}
		}
		
		private float GetSegmentAngle(int index)
		{
			return index * m_ArcAngle;
		}
	}
}