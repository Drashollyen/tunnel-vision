using NUnit.Framework;
using TunnelVision.UI;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class BackgroundFillTests
    {
        private static readonly Vector2 MenuArt = new Vector2(2.67f, 1.5f);

        [Test]
        public void WideScreen_FillsTheWidth()
        {
            // A wide Game view (about 2.15:1) with an orthographic size of 5.
            var view = new Vector2(21.5f, 10f);
            var scale = BackgroundFill.CoverScale(MenuArt, view);
            Assert.GreaterOrEqual(MenuArt.x * scale, view.x - 0.001f, "No gap at the sides.");
            Assert.GreaterOrEqual(MenuArt.y * scale, view.y - 0.001f, "No gap at the top or bottom.");
        }

        [Test]
        public void NarrowScreen_FillsTheHeight()
        {
            var view = new Vector2(13.3f, 10f); // 4:3.
            var scale = BackgroundFill.CoverScale(MenuArt, view);
            Assert.AreEqual(view.y, MenuArt.y * scale, 0.001f, "Just tall enough.");
            Assert.GreaterOrEqual(MenuArt.x * scale, view.x, "And wide enough.");
        }

        [Test]
        public void NoSprite_LeavesTheScaleAlone()
        {
            Assert.AreEqual(1f, BackgroundFill.CoverScale(Vector2.zero, new Vector2(16f, 9f)));
        }
    }
}
