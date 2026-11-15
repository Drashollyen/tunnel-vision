using System.Collections;
using NUnit.Framework;
using TunnelVision.Level;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    /// <summary>4 lives to start; each checkpoint gives back up to 2, once, never past 4.</summary>
    public class CheckpointLivesTests
    {
        private PlayerLives _lives;
        private PlayerRespawn _respawn;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("CheckpointLivesTest");

            var player = new GameObject("Player");
            player.AddComponent<Rigidbody2D>().gravityScale = 0f;
            _lives = player.AddComponent<PlayerLives>();
            _respawn = player.AddComponent<PlayerRespawn>();
            yield return null;
        }

        [TearDown]
        public void Clean()
        {
            IsolatedScene.Close();
        }

        private static Checkpoint MakeCheckpoint(float x)
        {
            var checkpoint = new GameObject("Checkpoint").AddComponent<Checkpoint>();
            checkpoint.transform.position = new Vector3(x, 0f, 0f);
            return checkpoint;
        }

        private void Lose(int count)
        {
            for (var i = 0; i < count; i++)
            {
                _lives.LoseLife();
            }
        }

        [Test]
        public void Player_StartsWithFourLives()
        {
            Assert.AreEqual(4, _lives.MaxLives);
            Assert.AreEqual(4, _lives.CurrentLives);
        }

        [Test]
        public void ThePlayerPrefab_HasFourLives()
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Player.prefab");
            Assert.AreEqual(4, prefab.GetComponent<PlayerLives>().MaxLives);
#endif
        }

        [Test]
        public void Checkpoint_GivesTwoLivesBack()
        {
            Lose(3);
            _respawn.ReachCheckpoint(MakeCheckpoint(10f));
            Assert.AreEqual(3, _lives.CurrentLives);
        }

        [Test]
        public void Checkpoint_NeverGoesPastFour()
        {
            Lose(1);
            _respawn.ReachCheckpoint(MakeCheckpoint(10f));
            Assert.AreEqual(4, _lives.CurrentLives);

            _respawn.ReachCheckpoint(MakeCheckpoint(20f));
            Assert.AreEqual(4, _lives.CurrentLives, "Already full.");
        }

        [Test]
        public void Checkpoint_OnlyPaysOutTheFirstTime()
        {
            var first = MakeCheckpoint(10f);
            var second = MakeCheckpoint(20f);
            Lose(3);

            _respawn.ReachCheckpoint(first);
            _respawn.ReachCheckpoint(second);
            Assert.AreEqual(4, _lives.CurrentLives);

            Lose(2);
            _respawn.ReachCheckpoint(first); // Walking back to an earlier one.
            _respawn.ReachCheckpoint(second);
            Assert.AreEqual(2, _lives.CurrentLives, "Neither should pay out again.");
        }

        [Test]
        public void LivesLost_CountsEveryDeath_EvenAfterLivesComeBack()
        {
            Lose(2);
            _respawn.ReachCheckpoint(MakeCheckpoint(10f));
            Lose(1);
            Assert.AreEqual(3, _lives.LivesLost);
            Assert.AreEqual(3, _lives.CurrentLives);

            _lives.Restart();
            Assert.AreEqual(0, _lives.LivesLost);
        }

        [Test]
        public void GainingLives_TellsTheHud()
        {
            Lose(2);
            var heard = -1;
            _lives.LivesChanged += lives => heard = lives;
            _respawn.ReachCheckpoint(MakeCheckpoint(10f));
            Assert.AreEqual(4, heard);
        }

        [Test]
        public void OutOfLives_ACheckpointCantBringYouBack()
        {
            Lose(4);
            Assert.AreEqual(0, _lives.GainLives(2));
            Assert.IsTrue(_lives.IsDead);
        }
    }
}
