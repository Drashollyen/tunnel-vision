using NUnit.Framework;
using TunnelVision.Level;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class RideParentingTests
    {
        private GameObject _ride;
        private GameObject _otherRide;
        private GameObject _rider;

        [SetUp]
        public void Build()
        {
            _ride = new GameObject("Seat");
            _otherRide = new GameObject("Next Seat");
            _rider = new GameObject("Player");
            _rider.transform.parent = _ride.transform;
        }

        [TearDown]
        public void Clean()
        {
            Object.DestroyImmediate(_rider);
            Object.DestroyImmediate(_ride);
            Object.DestroyImmediate(_otherRide);
        }

        [Test]
        public void SteppingOff_Unparents()
        {
            RideParenting.LetGo(_rider.transform, _ride.transform);
            Assert.IsNull(_rider.transform.parent);
        }

        [Test]
        public void AlreadyOnTheNextSeat_StaysThere()
        {
            _rider.transform.parent = _otherRide.transform;
            RideParenting.LetGo(_rider.transform, _ride.transform);
            Assert.AreSame(_otherRide.transform, _rider.transform.parent, "Leaving the old seat mustn't knock it off the new one.");
        }

        [Test]
        public void RideSwitchingOff_LeavesItAlone()
        {
            // Stopping Play mode or unloading the scene switches the seat off; Unity refuses a reparent then.
            _ride.SetActive(false);
            Assert.IsFalse(RideParenting.CanLetGo(_rider.transform, _ride.transform));
        }

        [Test]
        public void RiderSwitchingOff_LeavesItAlone()
        {
            _rider.SetActive(false);
            Assert.IsFalse(RideParenting.CanLetGo(_rider.transform, _ride.transform));
        }
    }
}
