namespace Watermelon
{
    public interface ICloudSavable
    {
        bool IsRequiredCloudSave { get; }
        string SaveCloudField { get; }
        string SaveCloudData();
        void LoadCloudData(string data);
    }
}
