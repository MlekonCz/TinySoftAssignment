using Core.Utility;

namespace GUI.Widgets
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Core;
	using Sirenix.OdinInspector;
	using UnityEngine;

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

		[SerializeField] private WheelSegmentWidget m_SegmentPrefab;
		[SerializeField] private Transform m_SegmentsParent;
		[SerializeField] private Transform m_RotationTransform;
		[SerializeField] private bool m_Clockwise = true;

		private float m_ArcAngle;
		
		public List<WheelSegmentWidget> WheelSegments { get; private set; } = new List<WheelSegmentWidget>();

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
			}
		}

		[Button]
		public void SetWheelRotationAngle(float angle)
		{
			m_RotationTransform.localRotation = Quaternion.Euler(0, 0, angle * GetClockwiseSign());
		}

		public float GetCurrentWheelRotationAngle()
		{
			return m_RotationTransform.localRotation.eulerAngles.z * GetClockwiseSign();
		}

		[Button]
		public void SetRotationToSegment(int index)
		{
			SetWheelRotationAngle(GetSegmentAngle(index));
		}

		[Button]
		public async Task AnimateRotationToSegment(int index)
		{
			int minRotationCycles = 2;
			float rotationTime = 2f;
			
			// TODO: Create smooth animation of WheelSpinning, with minimal rotation cycles and taking up rotation time.
			var finalAngle = GetSegmentAngle(index);
			while (rotationTime > 0 && m_OwnerScreen.IsActiveInStack)
			{
				rotationTime -= Time.deltaTime;
				SetWheelRotationAngle(GetCurrentWheelRotationAngle() + 500 * Time.deltaTime);
				await Task.Yield();
			}

			SetWheelRotationAngle(finalAngle);
		}

		private float GetSegmentAngle(int index)
		{
			return index * m_ArcAngle;
		}
		
		private float GetClockwiseSign()
		{
			return m_Clockwise ? -1f : 1f;
		}
	}
}