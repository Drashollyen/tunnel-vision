using NUnit.Framework;
using TunnelVision.Audio;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class AudioSourceExtensionsTests
    {
        [Test]
        public void UnsetSound_PlayAndStopDoNothing()
        {
            AudioSource missing = null;

            Assert.DoesNotThrow(() => missing.PlayIfSet());
            Assert.DoesNotThrow(() => missing.StopIfSet());
        }

        [Test]
        public void DestroyedSound_CountsAsUnset()
        {
            var holder = new GameObject("Sound");
            var source = holder.AddComponent<AudioSource>();
            Object.DestroyImmediate(holder);

            Assert.DoesNotThrow(() => source.PlayIfSet());
            Assert.DoesNotThrow(() => source.StopIfSet());
        }
    }
}
