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
    }
}
