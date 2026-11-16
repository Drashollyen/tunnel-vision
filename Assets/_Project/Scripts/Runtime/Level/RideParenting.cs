using UnityEngine;

namespace TunnelVision.Level
{
    /// <summary>
    /// Rides (wheel seats, lifts) carry the player by making it a child while it stands on them.
    /// This is the stepping-off half, shared so they all do it safely.
    /// </summary>
    public static class RideParenting
    {
        /// <summary>
        /// Unparents <paramref name="rider"/> from <paramref name="ride"/>, but only when it's
        /// still riding this one (it may already have stepped onto the next seat), and never while
        /// either is being switched on or off: Unity refuses to reparent then (it logs an error
        /// when Play mode stops or the scene unloads), and there's nothing to undo anyway.
        /// </summary>
        public static void LetGo(Transform rider, Transform ride)
        {
            if (!CanLetGo(rider, ride))
            {
                return;
            }

            rider.parent = null;
        }

        /// <summary>Whether <see cref="LetGo"/> would unparent. Public for tests.</summary>
        public static bool CanLetGo(Transform rider, Transform ride)
        {
            return rider != null && ride != null
                   && rider.parent == ride
                   && rider.gameObject.activeInHierarchy && ride.gameObject.activeInHierarchy;
        }
    }
}
