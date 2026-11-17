using UnityEngine;

namespace TunnelVision.Audio
{
    // Play and Stop that do nothing when the AudioSource is missing, so an object without one
    // (a test player, or a prefab that hasn't had one added yet) still works.
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
