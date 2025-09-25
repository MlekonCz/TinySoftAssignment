using System.Collections.Generic;
using Entities.Core.Scripts.Utility;

namespace Entities.Core.Scripts.User
{
    public static class SaveMapper
    {
        public static GameSave ToSave(UserData userData)
        {
            return new GameSave
            {
                AppStartCount = userData.AppStartCount,
                TimeInGame = userData.TimeInGame,
                
                Balance = userData.Balance,
                BetIndex = userData.BetIndex,

                recentResultsPerGame = userData.RecentResultsSave,
                winsPerGame = userData.WinsSave,
                losesPerGame = userData.LosesSave
            };
        }

        public static void Apply(GameSave saveData, UserData userData)
        {
            userData.SetBalance(saveData.Balance);
            userData.SetBetIndex(saveData.BetIndex);
            userData.AppStartCount = saveData.AppStartCount;
            userData.TimeInGame = saveData.TimeInGame;

            userData.RecentResultsSave = saveData.recentResultsPerGame ?? new SaveDictionary<string, List<float>>();
            userData.WinsSave = saveData.winsPerGame ?? new SaveDictionary<string, int>();
            userData.LosesSave = saveData.losesPerGame ?? new SaveDictionary<string, int>();

            userData.ClearDirty();
        }
    }
}