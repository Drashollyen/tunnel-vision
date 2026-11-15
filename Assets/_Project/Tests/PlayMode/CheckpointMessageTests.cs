using System.Collections;
using NUnit.Framework;
using TMPro;
using TunnelVision.Level;
using TunnelVision.Player;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    public class CheckpointMessageTests
    {
        private PlayerRespawn _respawn;
        private CheckpointMessage _message;
        private TMP_Text _text;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("CheckpointMessageTest");

            var player = new GameObject("Player");
            player.AddComponent<Rigidbody2D>().gravityScale = 0f;
            player.AddComponent<PlayerLives>();
            _respawn = player.AddComponent<PlayerRespawn>();

            var canvas = new GameObject("HUD", typeof(Canvas));
            var go = new GameObject("Checkpoint Message", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            _text = go.AddComponent<TextMeshProUGUI>();
            _message = go.AddComponent<CheckpointMessage>();
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

        [Test]
        public void Alpha_FadesInHoldsAndFadesOut()
        {
            Assert.AreEqual(0f, CheckpointMessage.AlphaAt(0f), 0.001f);
            Assert.AreEqual(0.5f, CheckpointMessage.AlphaAt(CheckpointMessage.FadeInSeconds * 0.5f), 0.001f);
            Assert.AreEqual(1f, CheckpointMessage.AlphaAt(CheckpointMessage.FadeInSeconds + 0.1f), 0.001f);
            Assert.Less(CheckpointMessage.AlphaAt(CheckpointMessage.TotalSeconds - 0.1f), 0.5f);
            Assert.AreEqual(0f, CheckpointMessage.AlphaAt(CheckpointMessage.TotalSeconds), 0.001f);
            Assert.AreEqual(0f, CheckpointMessage.AlphaAt(-1f), 0.001f);
        }

        [UnityTest]
        public IEnumerator StartsHidden_WithTheRightWords()
        {
            Assert.AreEqual(0f, _message.Visibility, 0.001f);
            Assert.AreEqual(CheckpointMessage.Message, _text.text);
            Assert.AreEqual("Checkpoint Reached", CheckpointMessage.Message);
            yield break;
        }

        [UnityTest]
        public IEnumerator ReachingACheckpoint_FadesTheMessageInThenOut()
        {
            _respawn.ReachCheckpoint(MakeCheckpoint(10f));
            yield return new WaitForSecondsRealtime(CheckpointMessage.FadeInSeconds + 0.2f);
            Assert.AreEqual(1f, _message.Visibility, 0.001f);

            yield return new WaitForSecondsRealtime(CheckpointMessage.HoldSeconds + CheckpointMessage.FadeOutSeconds);
            Assert.AreEqual(0f, _message.Visibility, 0.001f);
        }

        [UnityTest]
        public IEnumerator SameCheckpointAgain_DoesNotShowItAgain()
        {
            var checkpoint = MakeCheckpoint(10f);
            _respawn.ReachCheckpoint(checkpoint);
            yield return new WaitForSecondsRealtime(CheckpointMessage.TotalSeconds + 0.1f);

            _respawn.ReachCheckpoint(checkpoint);
            yield return new WaitForSecondsRealtime(CheckpointMessage.FadeInSeconds + 0.1f);

            Assert.AreEqual(0f, _message.Visibility, 0.001f);
        }

        [UnityTest]
        public IEnumerator NextCheckpoint_ShowsItAgain()
        {
            _respawn.ReachCheckpoint(MakeCheckpoint(10f));
            yield return new WaitForSecondsRealtime(CheckpointMessage.TotalSeconds + 0.1f);

            _respawn.ReachCheckpoint(MakeCheckpoint(50f));
            yield return new WaitForSecondsRealtime(CheckpointMessage.FadeInSeconds + 0.1f);

            Assert.AreEqual(1f, _message.Visibility, 0.001f);
        }
    }
}
