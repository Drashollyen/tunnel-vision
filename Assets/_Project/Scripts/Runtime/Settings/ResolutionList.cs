using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TunnelVision.Settings
{
    /// <summary>Builds the list of window sizes shown on the options page.</summary>
    public static class ResolutionList
    {
        private const int MinimumWidth = 800;
        private const int MinimumHeight = 600;

        /// <summary>
        /// Removes repeats (the same size at another refresh rate) and very small sizes, and always
        /// keeps <paramref name="current"/>. The largest size comes first.
        /// </summary>
        public static List<Vector2Int> Build(IEnumerable<Vector2Int> available, Vector2Int current)
        {
            return available
                .Where(size => size.x >= MinimumWidth && size.y >= MinimumHeight)
                .Append(current)
                .Distinct()
                .OrderByDescending(size => size.x)
                .ThenByDescending(size => size.y)
                .ToList();
        }

        public static string Label(Vector2Int size) => $"{size.x} x {size.y}";
    }
}
