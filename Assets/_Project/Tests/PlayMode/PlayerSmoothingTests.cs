using System.Collections;
using NUnit.Framework;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    /// <summary>
    /// The player glides (Rigidbody interpolation) when on their own, and switches it off while
    /// riding a wheel seat or moving platform, which parent the player to themselves.
    /// </summary>
    public class PlayerSmoothingTests
    {
        private Rigidbody2D _rb;
        private GameObject _player;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("PlayerSmoothingTest");

            _player = new GameObject("Player");
            _rb = _player.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.interpolation = RigidbodyInterpolation2D.None; // As both levels' scenes set it.

            _player.SetActive(false);
            _player.AddComponent<PlayerMovement>().rb = _rb;
            _player.SetActive(true);
            yield return null;
        }

        [TearDown]
        public void Clean()
        {
            IsolatedScene.Close();
        }

        [Test]
        public void Smoothing_IsOnAlone_AndOffWhileRiding()
        {
            Assert.AreEqual(RigidbodyInterpolation2D.Interpolate, PlayerMovement.SmoothingFor(riding: false));
            Assert.AreEqual(RigidbodyInterpolation2D.None, PlayerMovement.SmoothingFor(riding: true));
        }

        [UnityTest]
        public IEnumerator OnTheirOwn_ThePlayerIsSmoothed_EvenIfTheSceneTurnedItOff()
        {
            Assert.AreEqual(RigidbodyInterpolation2D.Interpolate, _rb.interpolation);
            yield break;
        }

        [UnityTest]
        public IEnumerator Riding_TurnsSmoothingOff_AndSteppingOffTurnsItBackOn()
        {
            var platform = new GameObject("Moving Platform");
            _player.transform.SetParent(platform.transform, true); // What a platform does when stood on.
            yield return null;
            Assert.AreEqual(RigidbodyInterpolation2D.None, _rb.interpolation, "Off while riding.");

            _player.transform.SetParent(null, true); // What a platform does when stepped off.
            yield return null;
            Assert.AreEqual(RigidbodyInterpolation2D.Interpolate, _rb.interpolation, "On again straight after.");
        }
    }
}
