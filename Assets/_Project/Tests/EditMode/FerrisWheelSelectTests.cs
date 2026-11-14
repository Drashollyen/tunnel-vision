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

        // The prototype has 3 levels. Cabins 4 and 5 (index 3 and 4) aren't in it.
        private const int PrototypeLevels = 3;

        [TestCase(0, 0, FerrisWheelSelect.CabinState.Playable)]
        [TestCase(1, 0, FerrisWheelSelect.CabinState.Locked)]
        [TestCase(1, 1, FerrisWheelSelect.CabinState.Playable)]
        [TestCase(2, 2, FerrisWheelSelect.CabinState.Playable)]
        [TestCase(3, 0, FerrisWheelSelect.CabinState.NotInPrototype)]
        [TestCase(4, 0, FerrisWheelSelect.CabinState.NotInPrototype)]
        public void GetCabinState_FollowsProgressAndPrototypeSize(
            int cabin, int highestUnlocked, FerrisWheelSelect.CabinState expected)
        {
            Assert.AreEqual(expected, FerrisWheelSelect.GetCabinState(cabin, PrototypeLevels, highestUnlocked));
        }

        [Test]
        public void GetCabinState_ExtraCabinsStayOutEvenWhenProgressIsAhead()
        {
            // Progress saved from a longer build mustn't open cabins the prototype doesn't have.
            Assert.AreEqual(FerrisWheelSelect.CabinState.NotInPrototype,
                FerrisWheelSelect.GetCabinState(4, PrototypeLevels, 10));
        }

        [Test]
        public void PlateText_ShowsNameLockedOrQuestionMark()
        {
            Assert.AreEqual("Level 1: Big Top",
                FerrisWheelSelect.PlateText(0, FerrisWheelSelect.CabinState.Playable, "Big Top"));
            Assert.AreEqual("Locked",
                FerrisWheelSelect.PlateText(1, FerrisWheelSelect.CabinState.Locked, "Stalls"));
            Assert.AreEqual("Level 4: ?",
                FerrisWheelSelect.PlateText(3, FerrisWheelSelect.CabinState.NotInPrototype, "Rides"));
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
