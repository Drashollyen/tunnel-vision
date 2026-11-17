using UnityEngine;

namespace TunnelVision.Level
{
    // Rides (wheel seats, moving platforms) carry the player by making the player their child.
    // This handles stepping off safely, and is shared by all of them.
    public static class RideParenting
    {
        // Only lets go if the player is still on this ride (they may already be on the next seat).
        // Also skips it while either object is being turned off, because Unity throws an error if
        // you change parents then (for example when Play mode stops or the scene unloads).
        public static void LetGo(Transform rider, Transform ride)
        {
            if (!CanLetGo(rider, ride))
            {
                return;
            }

            rider.parent = null;
        }

        // Public so tests can call it.
        public static bool CanLetGo(Transform rider, Transform ride)
        {
            return rider != null && ride != null
                   && rider.parent == ride
                   && rider.gameObject.activeInHierarchy && ride.gameObject.activeInHierarchy;
        }
    }
}
