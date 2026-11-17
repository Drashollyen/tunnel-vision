namespace TunnelVision.Settings
{
    // Where settings are saved. An interface so tests can use a fake in-memory store instead of PlayerPrefs.
    public interface ISettingsStore
    {
        float GetFloat(string key, float defaultValue);

        void SetFloat(string key, float value);

        int GetInt(string key, int defaultValue);

        void SetInt(string key, int value);

        void Save();
    }
}
