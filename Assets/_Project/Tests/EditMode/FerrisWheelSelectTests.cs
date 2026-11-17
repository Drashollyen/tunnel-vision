using NUnit.Framework;
using TunnelVision.UI;

namespace TunnelVision.Tests
{
    public class FerrisWheelSelectTests
    {
        private const float Step = 72f;

        [Test]
        public void NextStep_FromRest_MovesOne()
        {
            Assert.AreEqual(1, FerrisWheelSelect.NextStep(0, 0f, Step, 1));
            Assert.AreEqual(-1, FerrisWheelSelect.NextStep(0, 0f, Step, -1));
        }

        [Test]
        public void NextStep_Spammed_NeverQueuesMoreThanOneAhead()
        {
            var steps = 0;
            for (var i = 0; i < 10; i++)
            {
                steps = FerrisWheelSelect.NextStep(steps, 0f, Step, 1);
            }

            Assert.AreEqual(1, steps);
        }

        [Test]
        public void NextStep_Reversing_TurnsBackStraightAway()
        {
            var steps = FerrisWheelSelect.NextStep(0, 0f, Step, 1);
            steps = FerrisWheelSelect.NextStep(steps, 10f, Step, -1);
            Assert.AreEqual(0, steps);
        }

        [Test]
        public void StepsTo_ClickedCabin_TurnsTheShortWayRound()
        {
            Assert.AreEqual(0, FerrisWheelSelect.StepsTo(2, 2, 5), "Already at the top.");
            Assert.AreEqual(1, FerrisWheelSelect.StepsTo(0, 1, 5));
            Assert.AreEqual(-1, FerrisWheelSelect.StepsTo(0, 4, 5), "Cabin 5 is one step back, not four forward.");
            Assert.AreEqual(2, FerrisWheelSelect.StepsTo(4, 1, 5), "Wraps past the last cabin.");
            Assert.AreEqual(-2, FerrisWheelSelect.StepsTo(1, 4, 5));
        }

        // The prototype has 3 levels. Cabins 4 and 5 (index 3 and 4) aren't in it.
        private const int PrototypeLevels = 3;

        [TestCase(0, 0, FerrisWheelSelect.CabinState.Playable)]
        [TestCase(1, 0, FerrisWheelSelect.CabinState.Locked)]
        [TestCase(1, 1, FerrisWheelSelect.CabinState.Playable)]
        [TestCase(2, 2, FerrisWheelSelect.CabinState.Playable)]
        [TestCase(3, 0, FerrisWheelSelect.CabinState.NotInPrototype)]
        [TestCase(4, 0, FerrisWheelSelect.CabinState.NotInPrototype)]
        // Progress saved from a longer build mustn't open cabins the prototype doesn't have.
        [TestCase(4, 10, FerrisWheelSelect.CabinState.NotInPrototype)]
        public void GetCabinState_FollowsProgressAndPrototypeSize(
            int cabin, int highestUnlocked, FerrisWheelSelect.CabinState expected)
        {
            Assert.AreEqual(expected, FerrisWheelSelect.GetCabinState(cabin, PrototypeLevels, highestUnlocked));
        }

        [Test]
        public void PlateText_ShowsNameLockedOrQuestionMark()
        {
            Assert.AreEqual("Level 1: Big Top",
                FerrisWheelSelect.PlateText(0, FerrisWheelSelect.CabinState.Playable, "Big Top"));
            Assert.AreEqual("Locked",
                FerrisWheelSelect.PlateText(1, FerrisWheelSelect.CabinState.Locked, "Stalls"));
            Assert.AreEqual("Level 4: ???",
                FerrisWheelSelect.PlateText(3, FerrisWheelSelect.CabinState.NotInPrototype, "Rides"));
        }

        [Test]
        public void WobbleAngle_StartsAndEndsLevel()
        {
            Assert.AreEqual(0f, FerrisWheelSelect.WobbleAngle(0f), 0.001f);
            Assert.AreEqual(0f, FerrisWheelSelect.WobbleAngle(FerrisWheelSelect.WobbleSeconds), 0.001f);
            Assert.AreEqual(0f, FerrisWheelSelect.WobbleAngle(5f));
            Assert.AreEqual(0f, FerrisWheelSelect.WobbleAngle(-1f));
        }

        [Test]
        public void WobbleAngle_SwingsBothWaysAndStaysSubtle()
        {
            var most = 0f;
            var least = 0f;
            for (var t = 0f; t < FerrisWheelSelect.WobbleSeconds; t += 0.005f)
            {
                var angle = FerrisWheelSelect.WobbleAngle(t);
                most = UnityEngine.Mathf.Max(most, angle);
                least = UnityEngine.Mathf.Min(least, angle);
            }

            Assert.Greater(most, 2f, "It should tip far enough to see.");
            Assert.Less(least, -2f, "It should swing back the other way too.");
            Assert.LessOrEqual(most, 10f, "It should stay subtle.");
        }

        [Test]
        public void WobbleAngle_FadesOut()
        {
            // Compare the biggest tip in the first and last quarter of the wobble.
            float Peak(float from, float to)
            {
                var peak = 0f;
                for (var t = from; t < to; t += 0.002f)
                {
                    peak = UnityEngine.Mathf.Max(peak, UnityEngine.Mathf.Abs(FerrisWheelSelect.WobbleAngle(t)));
                }

                return peak;
            }

            var length = FerrisWheelSelect.WobbleSeconds;
            Assert.Less(Peak(length * 0.75f, length), Peak(0f, length * 0.25f));
        }

        [Test]
        public void NotInPrototypeMessage_IsTitleCase()
        {
            foreach (var word in FerrisWheelSelect.NotInPrototypeMessage.Split(' '))
            {
                Assert.That(char.IsUpper(word[0]), Is.True, $"\"{word}\" should start with a capital.");
            }
        }
    }
}
