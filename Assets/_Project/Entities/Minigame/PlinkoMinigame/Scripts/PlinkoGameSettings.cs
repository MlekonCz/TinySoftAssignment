using Core;
using UnityEngine;

namespace _Project.Entities.Minigame.PlinkoMinigame.Scripts
{
	[CreateAssetMenu(fileName = "PlinkoGameSettings", menuName = "Settings/Games/PlinkoGameSettings")]
    public class PlinkoGameSettings : MiniGameSettingsBase<PlinkoGameSettings, PlinkoGame, PlinkoGameScreen, long, PlinkoWidget.PlinkoBoxConfig>
    {
        public PlinkoWidget.PlinkoConfig PlinkoConfig;

        protected override PlinkoGame CreateTypedGame(UserService userService)
        {
            return new PlinkoGame(this, userService);
        }
    }	
}