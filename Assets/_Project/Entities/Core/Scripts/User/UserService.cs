using Sirenix.OdinInspector;

namespace Entities.Core.Scripts.User
{
    public class UserService : IInitializable, IUpdatable
    {
        [ShowInInspector] public UserData UserData { get; private set; } = new();

        private readonly ISaveRepository m_Repo;
        private UserSettings m_UserSettings;

        private bool m_Pending;
        private float m_Timer;
        private const float SAVE_DELAY = 0.8f;

        private float m_PlaytimeAccum; 

        private float m_AutoSaveTimer;
        private const float AUTO_SAVE_INTERVAL = 15f;

        public UserService(ISaveRepository repo)
        {
            m_Repo = repo;
        }

        void IInitializable.Initialize(ServiceLocator locator)
        {
            m_UserSettings = locator.Get<MasterSettings>().UserSettings;

            var save = m_Repo.LoadOrDefault();
            SaveMapper.Apply(save, UserData);

            if (UserData.AppStartCount == 0)
            {
                SetStartingUserData();
                MarkDirty();
            }
        }

        void IInitializable.Deinitialize()
        {
            SaveNow();
        }

        void IUpdatable.Update(float deltaTime)
        {
            m_PlaytimeAccum += deltaTime;
            var wholeSeconds = (long)m_PlaytimeAccum;
            if (wholeSeconds > 0)
            {
                UserData.TimeInGame += wholeSeconds;
                m_PlaytimeAccum -= wholeSeconds;
            }
            
            m_AutoSaveTimer += deltaTime;
            if (m_AutoSaveTimer >= AUTO_SAVE_INTERVAL)
            {
                m_AutoSaveTimer = 0f;
                SaveNow();
            }
            
            if (UserData.Dirty)
            {
                m_Pending = true;
                m_Timer = 0f;
            }

            if (!m_Pending) return;

            m_Timer += deltaTime;
            if (m_Pending && UserData.Dirty && m_Timer >= SAVE_DELAY)
                SaveNow();
        }

        public void ResetUser()
        {
            m_Repo.Delete();
            UserData.ResetData();
            SetStartingUserData();
            MarkDirty();
            SaveNow();
        }

        private void SetStartingUserData()
        {
            UserData.SetBalance(m_UserSettings.StartBalance);
        }

        private void MarkDirty()
        {
            m_Pending = true;
            m_Timer = 0f;
        }

        private void SaveNow()
        {
            m_Pending = false;
            m_Timer = 0f;

            m_Repo.Save(SaveMapper.ToSave(UserData));
            UserData.ClearDirty();
        }
    }
}