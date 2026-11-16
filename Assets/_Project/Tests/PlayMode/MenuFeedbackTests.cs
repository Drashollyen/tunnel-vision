using System.Collections;
using System.Linq;
using NUnit.Framework;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TunnelVision.Tests
{
    /// <summary>Small menu touches: buttons growing on hover, and the level select wheel's chasing lights.</summary>
    public class MenuFeedbackTests
    {
        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("MenuFeedbackTest");
            new GameObject("EventSystem", typeof(EventSystem));
        }

        [TearDown]
        public void Clean()
        {
            IsolatedScene.Close();
        }

        private static (Button button, ButtonHover hover) MakeButton()
        {
            var canvas = new GameObject("Canvas", typeof(Canvas));
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(canvas.transform, false);
            return (go.GetComponent<Button>(), go.AddComponent<ButtonHover>());
        }

        private static IEnumerator Settle()
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }

        [UnityTest]
        public IEnumerator Button_GrowsWhileHovered_AndShrinksBack()
        {
            var (button, hover) = MakeButton();
            var pointer = new PointerEventData(EventSystem.current);

            hover.OnPointerEnter(pointer);
            yield return Settle();
            Assert.AreEqual(1.06f, button.transform.localScale.x, 0.005f, "Slightly bigger under the mouse.");

            hover.OnPointerExit(pointer);
            yield return Settle();
            Assert.AreEqual(1f, button.transform.localScale.x, 0.005f, "Back to normal when the mouse leaves.");
        }

        [UnityTest]
        public IEnumerator Button_ClickSelection_DoesNotLeaveItEnlarged()
        {
            var (button, hover) = MakeButton();
            hover.OnSelect(new PointerEventData(EventSystem.current)); // A mouse click selects it too.
            yield return Settle();
            Assert.AreEqual(1f, button.transform.localScale.x, 0.005f);

            hover.OnSelect(new BaseEventData(EventSystem.current)); // The keyboard, though, does raise it.
            yield return Settle();
            Assert.AreEqual(1.06f, button.transform.localScale.x, 0.005f);
        }

        [UnityTest]
        public IEnumerator DisabledButton_DoesNotReact()
        {
            var (button, hover) = MakeButton();
            button.interactable = false;
            hover.OnPointerEnter(new PointerEventData(EventSystem.current));
            yield return Settle();
            Assert.AreEqual(1f, button.transform.localScale.x, 0.005f);
        }

        [UnityTest]
        public IEnumerator WheelLights_LightEveryThirdBulb_AndMoveAlong()
        {
            var wheel = new GameObject("Lights", typeof(RectTransform));
            wheel.SetActive(false);
            var bulbs = Enumerable.Range(0, 9).Select(i =>
            {
                var bulb = new GameObject("Bulb " + i, typeof(RectTransform), typeof(Image));
                bulb.transform.SetParent(wheel.transform, false);
                return bulb.GetComponent<Image>();
            }).ToArray();
            var lights = wheel.AddComponent<WheelLights>();
            wheel.SetActive(true);
            yield return null;

            var lit = new Color(1f, 0.92f, 0.5f);
            int[] LitNow() => Enumerable.Range(0, bulbs.Length).Where(i => bulbs[i].color == lit).ToArray();

            var first = LitNow();
            Assert.AreEqual(3, first.Length, "One bulb in every three is lit.");
            Assert.IsTrue(first.All(i => i % 3 == first[0] % 3), "Evenly spaced.");

            yield return new WaitForSecondsRealtime(0.3f); // More than one step at 4 a second.
            Assert.AreNotEqual(first[0] % 3, LitNow()[0] % 3, "The lights chase along.");
        }
    }
}
