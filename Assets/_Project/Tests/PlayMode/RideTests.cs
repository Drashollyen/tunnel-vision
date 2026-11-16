using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TunnelVision.Level;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using static TunnelVision.Tests.Private;

namespace TunnelVision.Tests
{
    /// <summary>The fairground rides: lifts, the big wheel's seats and the drop tower, built bare.</summary>
    public class RideTests
    {
        private GameObject _player;
        private int _deaths;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("RideTest");
            _player = new GameObject("Player") { tag = "Player" };
            _player.transform.position = new Vector3(100f, 100f, 0f);
            _player.AddComponent<Rigidbody2D>().gravityScale = 0f;
            _player.AddComponent<BoxCollider2D>();
            _player.AddComponent<PlayerLives>();
            var respawn = _player.AddComponent<PlayerRespawn>();
            _deaths = 0;
            respawn.Died += () => _deaths++;
        }

        [TearDown]
        public void Clean()
        {
            Object.DestroyImmediate(_player);
            IsolatedScene.Close();
        }

        private static Transform Point(string name, Vector3 position, Transform parent = null)
        {
            var point = new GameObject(name).transform;
            point.SetParent(parent, false);
            point.position = position;
            return point;
        }

        private void MovePlayerTo(Vector2 position)
        {
            _player.transform.position = position;
            Physics2D.SyncTransforms();
        }

        [UnityTest]
        public IEnumerator Lift_GoesUpToItsTopPoint_ThenBackDown()
        {
            var go = new GameObject("Lift");
            go.SetActive(false);
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            var lift = go.AddComponent<MovingPlatform>();
            lift.pointA = Point("Bottom", Vector3.zero);
            lift.pointB = Point("Top", new Vector3(0f, 3f, 0f));
            lift.moveSpeed = 10f; // Up in 0.3 s, then down by 0.6 s.
            go.SetActive(true);

            var highest = 0f;
            for (var until = Time.time + 0.45f; Time.time < until;)
            {
                highest = Mathf.Max(highest, go.transform.position.y);
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(3f, highest, 0.05f, "It should reach the top point.");
            Assert.Less(go.transform.position.y, highest, "Then head back down.");
        }

        [UnityTest]
        public IEnumerator WheelSeat_GoesRoundTheHub_AndStaysLevel()
        {
            var hub = Point("Hub", Vector3.zero);
            var seat = new GameObject("Seat");
            seat.SetActive(false);
            seat.transform.position = new Vector3(8f, 0f, 0f);
            var rotation = seat.AddComponent<BigWheelRotation>();
            rotation.transRotationPoint = hub;
            Set(rotation, "fRotationSpeed", 90f);
            seat.SetActive(true);

            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(8f, Vector2.Distance(seat.transform.position, hub.position), 0.01f, "Same distance from the hub.");
            Assert.Greater(Vector2.Distance(seat.transform.position, new Vector2(8f, 0f)), 1f, "It has moved round.");
            Assert.AreEqual(0f, Mathf.DeltaAngle(0f, seat.transform.eulerAngles.z), 0.5f, "The seat stays level, so he can stand on it.");
        }

        private DropTower MakeDropTower(out Transform seat, out GameObject crush)
        {
            var go = new GameObject("Drop Tower");
            go.SetActive(false);
            var tower = go.AddComponent<DropTower>();
            seat = Point("Seats", new Vector3(0f, 7f, 0f), go.transform);
            var bottom = Point("Bottom", new Vector3(0f, -2f, 0f), go.transform);
            crush = new GameObject("Crush");
            crush.transform.SetParent(seat, false);
            crush.AddComponent<BoxCollider2D>().isTrigger = true;
            crush.AddComponent<DropTowerCrush>();
            Set(tower, "seatPlatform", seat.gameObject);
            Set(tower, "bottomPoint", bottom);
            Set(tower, "fallSpeed", 100f);
            Set(tower, "returnSpeed", 100f);
            Set(tower, "waitDuration", 0.1f);
            go.SetActive(true);
            return tower;
        }

        private static void SetTowerState(DropTower tower, string state)
        {
            var field = typeof(DropTower).GetField("currentState", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(tower, Enum.Parse(field.FieldType, state));
        }

        [UnityTest]
        public IEnumerator DropTower_FallsWhenNicholasStepsUnder_ThenRisesBack()
        {
            var tower = MakeDropTower(out var seat, out _);
            var trigger = tower.gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(2f, 2f);
            yield return null;
            Assert.IsFalse(tower.IsFalling, "Waits at the top.");

            MovePlayerTo(Vector2.zero); // Inside the tower's trigger.
            for (var until = Time.time + 1f; Time.time < until && !tower.IsFalling;)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.IsTrue(tower.IsFalling, "Stepping under it sets it off.");
            MovePlayerTo(new Vector2(100f, 100f));

            var lowest = seat.position.y;
            for (var until = Time.time + 0.5f; Time.time < until;)
            {
                lowest = Mathf.Min(lowest, seat.position.y);
                yield return null;
            }

            Assert.AreEqual(-2f, lowest, 0.01f, "It drops to the bottom.");
            Assert.AreEqual(7f, seat.position.y, 0.01f, "Then climbs back to the top.");
        }

        [UnityTest]
        public IEnumerator DropTowerCrush_OnlyHurtsWhileFalling()
        {
            var tower = MakeDropTower(out var seat, out var crush);
            yield return null;

            MovePlayerTo(crush.transform.position);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(0, _deaths, "Touching the seats while they wait is safe.");

            MovePlayerTo(new Vector2(100f, 100f));
            yield return new WaitForFixedUpdate();
            Set(tower, "fallSpeed", 0.5f); // Slow, so the seats are still over him when physics checks.
            SetTowerState(tower, "Falling");
            seat.position = new Vector3(0f, 0f, 0f);
            MovePlayerTo(Vector2.zero);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(1, _deaths, "Being under it as it falls costs a life.");
        }
    }
}
