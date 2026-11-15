using UnityEngine;

namespace TunnelVision.Audio
{
    /// <summary>
    /// Play and Stop that do nothing when the sound isn't set, so an object without its
    /// AudioSource (a test player, or a prefab that hasn't had one added) still works.
    /// </summary>
    public static class AudioSourceExtensions
    {
        public static void PlayIfSet(this AudioSource source)
        {
            if (source != null)
            {
                source.Play();
            }
        }

        public static void StopIfSet(this AudioSource source)
        {
            if (source != null)
            {
                source.Stop();
            }
        }
    }
}
