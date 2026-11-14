using NUnit.Framework;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class PlayerCollectiblesTests
    {
        private GameObject _player;
        private PlayerCollectibles _collectibles;

        [SetUp]
        public void Make()
        {
            _player = new GameObject("Player");
            _collectibles = _player.AddComponent<PlayerCollectibles>();
        }

        [TearDown]
        public void Clean()
        {
            Object.DestroyImmediate(_player);
        }

        [Test]
        public void StartsAtZero()
        {
            Assert.AreEqual(0, _collectibles.Count);
        }

        [Test]
        public void Add_GoesUpByOneEachTime()
        {
            _collectibles.Add();
            _collectibles.Add();
            _collectibles.Add();

            Assert.AreEqual(3, _collectibles.Count);
        }

        [Test]
        public void Add_WithAnAmount_AddsThatMany()
        {
            _collectibles.Add(5);

            Assert.AreEqual(5, _collectibles.Count);
        }

        [Test]
        public void Add_ZeroOrLess_IsIgnored()
        {
            _collectibles.Add(2);
            _collectibles.Add(0);
            _collectibles.Add(-3);

            Assert.AreEqual(2, _collectibles.Count);
        }

        [Test]
        public void Add_TellsListenersTheNewCount()
        {
            var heard = -1;
            var calls = 0;
            _collectibles.CountChanged += count =>
            {
                heard = count;
                calls++;
            };

            _collectibles.Add();
            _collectibles.Add(0); // Ignored, so no event.

            Assert.AreEqual(1, heard);
            Assert.AreEqual(1, calls);
        }
    }
}
