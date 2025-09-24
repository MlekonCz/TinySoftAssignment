using System;
using System.Collections.Generic;
using Entities.Core.Scripts.Utility;

namespace Entities.Core.Scripts.User
{
    [Serializable]
    public class GameSave
    {
        public int AppStartCount;
        public long TimeInGame;

        public long Balance;
        public int BetIndex;

        public SaveDictionary<string, List<float>> recentResultsPerGame = new();
        public SaveDictionary<string, float> winsPerGame = new();
        public SaveDictionary<string, float> losesPerGame = new();
    }
}