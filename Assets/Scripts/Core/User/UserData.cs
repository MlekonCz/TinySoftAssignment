namespace Core
{
	using System;
	using Sirenix.OdinInspector;
	using UnityEngine;

	[Serializable]
	public class UserData
	{
		// Serialized fields
		
		[SerializeField, HideInInspector] private long m_Balance;
		[SerializeField, HideInInspector] private int m_BetIndex;

		
		// Public properties (getters)
		[ShowInInspector, ReadOnly]  public long Balance => m_Balance;
		[ShowInInspector, ReadOnly]  public int BetIndex => m_BetIndex;

		public bool Dirty { get; private set; }
		
		
		// Signals
		public Action SignalBalanceChanged { get; set; }
		public Action SignalBetIndexChanged { get; set; }

		
		// Data changing methods
		[Button]
		public void SetBalance(long value)
		{
			m_Balance = value;
			AfterBalanceChanged();
		}
		
		[Button]
		public void AddBalance(long value)
		{
			m_Balance += value;
			AfterBalanceChanged();
		}

		[Button]
		public void SetBetIndex(int index)
		{
			m_BetIndex = index;
			AfterBetChanged();
		}

		public void ClearDirty()
		{
			SetDirty(false);
		}
		
		
		// private methods
		private void AfterBalanceChanged()
		{
			if (m_Balance < 0)
			{
				Debug.LogError("Balance got negative - that shouldn't happen!");
				m_Balance = 0;
			}
			
			if (SignalBalanceChanged != null)
			{
				SignalBalanceChanged.Invoke();
			}

			SetDirty();
		}

		private void AfterBetChanged()
		{
			if (m_BetIndex < 0)
			{
				Debug.LogError("Bet index got negative - that shouldn't happen!");
				m_BetIndex = 0;
			}
			
			if (SignalBetIndexChanged != null)
			{
				SignalBetIndexChanged.Invoke();
			}
			
			SetDirty();
		}
		
		private void SetDirty(bool dirty = true)
		{
			Dirty = dirty;
		}
	}
}