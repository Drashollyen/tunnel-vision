using NUnit.Framework;
using TunnelVision.Level;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TunnelVision.Tests
{
    public class StripedTileTests
    {
        private StripedTile _tile;

        [SetUp]
        public void CreateTile()
        {
            _tile = ScriptableObject.CreateInstance<StripedTile>();
        }

        [TearDown]
        public void Clean()
        {
            Object.DestroyImmediate(_tile);
        }

        private TileData DataAt(int x)
        {
            var data = new TileData();
            _tile.GetTileData(new Vector3Int(x, 0, 0), null, ref data);
            return data;
        }

        [Test]
        public void ColumnsAlternateColours()
        {
            Assert.AreEqual(Color.red, DataAt(2).color);
            Assert.AreEqual(Color.white, DataAt(3).color);
        }

        [Test]
        public void NegativeColumnsStillAlternate()
        {
            Assert.AreEqual(Color.white, DataAt(-1).color);
            Assert.AreEqual(Color.red, DataAt(-2).color);
        }

        [Test]
        public void TileIsSolid()
        {
            Assert.AreEqual(Tile.ColliderType.Grid, DataAt(0).colliderType);
        }
    }
}
