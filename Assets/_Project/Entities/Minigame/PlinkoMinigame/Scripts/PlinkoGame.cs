using Core;
using Core.Utility;
using GUI.Widgets;
using WheelOfFortune;

namespace _Project.Entities.Minigame.PlinkoMinigame.Scripts
{
    public class PlinkoGame : MiniGameBase<PlinkoGameSettings, long, PlinkoWidget.PlinkoBoxConfig>
    {
        public PlinkoGame(PlinkoGameSettings settings, UserService userService) : base(settings, userService)
        {
        }

        public override bool TryToPlaceBet(long bet)
        {
            if (m_UserData.Balance < bet)
                return false;

            m_UserData.AddBalance(-bet);
            return true;
        }

        public override PlinkoWidget.PlinkoBoxConfig GenerateWinResult() => Settings.PlinkoConfig.Box.SelectWeightedRandom(m_Random);

        public override void ProcessWinResult(long bet, PlinkoWidget.PlinkoBoxConfig winResult) => m_UserData.AddBalance(bet * winResult.WinPercent / 100);
    }
}