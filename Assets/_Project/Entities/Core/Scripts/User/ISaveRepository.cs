namespace Entities.Core.Scripts.User
{
    public interface ISaveRepository 
    {
        void Save(GameSave data);
        GameSave LoadOrDefault();
        void Delete();
    }
}