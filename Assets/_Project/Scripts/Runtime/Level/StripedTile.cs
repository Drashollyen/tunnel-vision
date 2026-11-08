using UnityEngine;
using UnityEngine.Tilemaps;

namespace TunnelVision.Level
{
    /// <summary>
    /// Paints vertical stripes with one brush: each column of the tilemap alternates
    /// between the two colours. Used for the Tent zone's striped platforms.
    /// </summary>
    [CreateAssetMenu(menuName = "Tunnel Vision/Striped Tile")]
    public class StripedTile : TileBase
    {
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color evenColumns = Color.red;
        [SerializeField] private Color oddColumns = Color.white;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = sprite;
            tileData.color = (position.x & 1) == 0 ? evenColumns : oddColumns;
            tileData.colliderType = Tile.ColliderType.Grid;
            tileData.flags = TileFlags.LockColor;
            tileData.transform = Matrix4x4.identity;
        }
    }
}
