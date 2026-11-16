using System.Collections;
using NUnit.Framework;
using TMPro;
using TunnelVision.Player;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    public class CollectibleMessageTests
    {
        private PlayerCollectibles _collectibles;
        private CollectibleMessage _message;
        private TMP_Text _text;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("CollectibleMessageTest");

            _collectibles = new GameObject("Player").AddComponent<PlayerCollectibles>();

            var canvas = new GameObject("HUD", typeof(Canvas));
            var go = new GameObject("Collectible Message", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            _text = go.AddComponent<TextMeshProUGUI>();
            _message = go.AddComponent<CollectibleMessage>();
            yield return null;
        }

        [TearDown]
        public void Clean()
        {
            IsolatedScene.Close();
        }

        [UnityTest]
        public IEnumerator StartsHidden_WithTheRightWords()
        {
            Assert.AreEqual(0f, _message.Visibility, 0.001f);
            Assert.AreEqual("Teddy Found", _text.text);
            yield break;
        }

        [UnityTest]
        public IEnumerator PickingUpATeddy_FadesTheMessageInThenOut()
        {
            _collectibles.Add();
            yield return new WaitForSecondsRealtime(CheckpointMessage.FadeInSeconds + 0.2f);
            Assert.AreEqual(1f, _message.Visibility, 0.001f);

            yield return new WaitForSecondsRealtime(CheckpointMessage.HoldSeconds + CheckpointMessage.FadeOutSeconds);
            Assert.AreEqual(0f, _message.Visibility, 0.001f);
        }

        [UnityTest]
        public IEnumerator NextTeddy_ShowsItAgain()
        {
            _collectibles.Add();
            yield return new WaitForSecondsRealtime(CheckpointMessage.TotalSeconds + 0.1f);

            _collectibles.Add();
            yield return new WaitForSecondsRealtime(CheckpointMessage.FadeInSeconds + 0.1f);

            Assert.AreEqual(1f, _message.Visibility, 0.001f);
        }
    }
}
