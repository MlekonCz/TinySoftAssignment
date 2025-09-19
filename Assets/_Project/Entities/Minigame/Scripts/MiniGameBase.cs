namespace Core
{
	using System;
	using UnityEngine;
	using Utility;

	public abstract class MiniGameBase : IDisposable
	{
		protected UserData m_UserData;
		protected SeedRandom m_Random;

		public MiniGameBase(UserService userService)
		{
			m_UserData = userService.UserData;
			m_Random = new SeedRandom();
		}

		public abstract void Start();
		public abstract void Dispose();
	}
	
	
	
	public abstract class MiniGameBase<TSettings, TBet, TWinResult> : MiniGameBase where TSettings: MiniGameSettingsBase
	{
		public readonly TSettings Settings;
		
		public MiniGameBase(TSettings settings, UserService userService) : base(userService)
		{
			Settings = settings;
		}

		public override void Start()
		{
			Debug.Log("MiniGame started: " + Settings.GameName);
		}
		
		public override void Dispose()
		{
			Debug.Log("MiniGame ended: " + Settings.GameName);
		}

		public abstract bool TryToPlaceBet(TBet bet);
		public abstract TWinResult GenerateWinResult();
		public abstract void ProcessWinResult(TBet bet, TWinResult winResult);
	}
}