namespace Watermelon
{
    public interface ISavable
    {
        SavableObject SaveObject();
        void LoadObject(SavableObject savableObject);
    }

    [System.Serializable]
    public class SavableObject { }
}