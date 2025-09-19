namespace GUI
{
	using System.Collections.Generic;
	using Core;
	using UnityEngine;

	public class GamesHubScreen : ScreenView
	{
		public override bool CanBeClosed => false;
		
		[SerializeField] private Transform m_GamesContainer;
		
		private List<MiniGameSettingsBase> m_GameList;
		private UserService m_UserService;

		internal override void Initialize(ScreenStack stack, ServiceLocator locator)
		{
			base.Initialize(stack, locator);

			m_GameList = locator.Get<MasterSettings>().MiniGamesSettings.MiniGameList;
			m_UserService = locator.Get<UserService>();

			RenderGameList();
		}

		private void RenderGameList()
		{
			foreach (var game in m_GameList)
			{
				var thumbnail = Instantiate(game.GameThumbnail, m_GamesContainer);
				thumbnail.Setup(game, RunGame);
			}
		}

		private void RunGame(MiniGameSettingsBase miniGameSettingsBaseGame)
		{
			var gameView = miniGameSettingsBaseGame.CreateAndOpenGameView(m_UserService, m_ScreenStack);
		}
	}
}
