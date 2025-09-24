using UnityEngine;

namespace Entities.Core.Scripts.User
{
    public class PlayerPrefsSaveRep : ISaveRepository
    {
        private readonly string m_key;

        public PlayerPrefsSaveRep(string mKey)
        {
            m_key = mKey;
        }

        public void Save(GameSave data) 
        {
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(m_key, json);
            PlayerPrefs.Save();
        }

        public GameSave LoadOrDefault() 
        {
            if (!PlayerPrefs.HasKey(m_key)) return new GameSave();
            var json = PlayerPrefs.GetString(m_key, "{}");
            var data = JsonUtility.FromJson<GameSave>(json) ?? new GameSave();
            return data;
        }

        public void Delete() 
        {
            if (PlayerPrefs.HasKey(m_key)) PlayerPrefs.DeleteKey(m_key);
            PlayerPrefs.Save();
        }
    }
}