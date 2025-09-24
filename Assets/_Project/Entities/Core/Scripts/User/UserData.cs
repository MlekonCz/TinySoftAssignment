using System;
using System.Collections.Generic;
using Entities.Core.Scripts.Utility;
using Entities.Core.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Entities.Core.Scripts.User
{
	[Serializable]
	public class UserData
	{
		private long m_Balance;
		private int m_BetIndex;
		
		// Public properties (getters)
		[ShowInInspector, ReadOnly]  public long Balance => m_Balance;
		[ShowInInspector, ReadOnly]  public int BetIndex => m_BetIndex;

		public bool Dirty { get; private set; }
		
		// Signals
		public Action SignalBalanceChanged { get; set; }
		public Action SignalBetIndexChanged { get; set; }
		
		private int m_AppStartCount;
		public int AppStartCount
		{
			get => m_AppStartCount;
			set
			{
				m_AppStartCount = value;
				SetDirty();
			}
		}

		private long m_TimeInGame;
		public long TimeInGame
		{
			get => m_TimeInGame;
			set => m_TimeInGame = value;
		}
		
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
		
		private SaveDictionary<string, List<float>> m_RecentResultsPerGame;
		private SaveDictionary<string, float> m_WinsPerGame;
		private SaveDictionary<string, float> m_LosesPerGame;

		private const int MAX_SIZE_OF_RESULT_HISTORY_PER_GAME = 100;
		
		public List<float> GetResultsPerGame(string id) => m_RecentResultsPerGame.GetOrDefault(id, new List<float>());
		public float GetWinsPerGame(string id) => m_WinsPerGame.GetOrDefault(id, 0f);
		public float GetLosesPerGame(string id) => m_LosesPerGame.GetOrDefault(id, 0f);

		public void SetResult(string gameId, float value)
		{
			if (string.IsNullOrEmpty(gameId)) return;

			if (value > 0) m_WinsPerGame.Set(gameId, value);
			else m_LosesPerGame.Set(gameId, value);

			m_RecentResultsPerGame.PushCapped(gameId, value, MAX_SIZE_OF_RESULT_HISTORY_PER_GAME);
			SetDirty();
		}

		public SaveDictionary<string, List<float>> RecentResultsSave
		{
			get => m_RecentResultsPerGame;
			set => m_RecentResultsPerGame = value ?? new();
		}

		public SaveDictionary<string, float> WinsSave
		{
			get => m_WinsPerGame;
			set => m_WinsPerGame = value ?? new();
		}

		public SaveDictionary<string, float> LosesSave
		{
			get => m_LosesPerGame;
			set => m_LosesPerGame = value ?? new();
		}

		private void SetDirty(bool dirty = true)
		{
			Dirty = dirty;
		}

		public void ResetData()
		{
			m_Balance = 0;
			m_BetIndex = 0;
			SetDirty();
		}
	}
}