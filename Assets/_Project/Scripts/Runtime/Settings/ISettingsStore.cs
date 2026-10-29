namespace TunnelVision.Settings
{
    /// <summary>
    /// Key/value storage used by <see cref="GameSettings"/>.
    /// Abstracted so tests can use an in-memory store instead of PlayerPrefs.
    /// </summary>
    public interface ISettingsStore
    {
        bool HasKey(string key);

        float GetFloat(string key, float defaultValue);

        void SetFloat(string key, float value);

        int GetInt(string key, int defaultValue);

        void SetInt(string key, int value);

        void Save();
    }
}
