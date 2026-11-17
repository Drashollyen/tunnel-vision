using UnityEngine;

namespace TunnelVision.Settings
{
    // The real settings store, saving with Unity's PlayerPrefs.
    public sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        public float GetFloat(string key, float defaultValue) => PlayerPrefs.GetFloat(key, defaultValue);

        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);

        public int GetInt(string key, int defaultValue) => PlayerPrefs.GetInt(key, defaultValue);

        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);

        public void Save() => PlayerPrefs.Save();
    }
}
