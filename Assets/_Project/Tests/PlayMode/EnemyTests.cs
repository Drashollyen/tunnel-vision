using System.Collections;
using NUnit.Framework;
using TunnelVision.Enemies;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.TestTools;
using static TunnelVision.Tests.Private;

namespace TunnelVision.Tests
{
    /// <summary>
    /// The enemies' behaviour, each built bare in a scene of its own with a stand-in Nicholas:
    /// patrolling, chasing, springing, throwing, and what happens when they touch him.
    /// </summary>
    public class EnemyTests
    {
        private const int GroundLayer = 6;

        private GameObject _player;
        private PlayerRespawn _respawn;
        private int _deaths;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("EnemyTest");
            _player = new GameObject("Player") { tag = "Player" };
            _player.transform.position = new Vector3(100f, 100f, 0f); // Far from everything until a test moves him.
            _player.AddComponent<Rigidbody2D>().gravityScale = 0f;
            _player.AddComponent<BoxCollider2D>();
            _player.AddComponent<PlayerLives>();
            _respawn = _player.AddComponent<PlayerRespawn>();
            _deaths = 0;
            _respawn.Died += () => _deaths++;
        }

        [TearDown]
        public void Clean()
        {
            Object.DestroyImmediate(_player); // Stops any death sequence a hit started.
            IsolatedScene.Close();
        }

        private static Transform Point(string name, float x, float y)
        {
            var point = new GameObject(name).transform;
            point.position = new Vector3(x, y, 0f);
            return point;
        }

        private static IEnumerator Physics(int steps = 3)
        {
            for (var i = 0; i < steps; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            yield return null;
        }

        private void MovePlayerTo(Vector2 position)
        {
            _player.transform.position = position;
            Physics2D.SyncTransforms();
        }

        // ---- Balloon dog ----

        private GameObject MakeBalloon(float speed)
        {
            var go = new GameObject("Balloon");
            go.SetActive(false);
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            var balloon = go.AddComponent<Balloon>();
            Set(balloon, "pointA", Point("A", 0f, 0f));
            Set(balloon, "pointB", Point("B", 2f, 0f));
            Set(balloon, "speed", speed);
            go.SetActive(true);
            return go;
        }

        [UnityTest]
        public IEnumerator Balloon_FloatsToItsFarPoint_ThenBack()
        {
            var balloon = MakeBalloon(10f);
            var furthest = 0f;
            for (var until = Time.time + 0.3f; Time.time < until;)
            {
                furthest = Mathf.Max(furthest, balloon.transform.position.x);
                yield return null;
            }

            Assert.AreEqual(2f, furthest, 0.15f, "It should reach point B.");
            yield return new WaitForSeconds(0.1f);
            Assert.Less(balloon.transform.position.x, furthest, "Then turn back toward point A.");
        }

        [UnityTest]
        public IEnumerator Balloon_Pops_WhenNicholasTouchesIt()
        {
            var balloon = MakeBalloon(0f);
            yield return null;
            MovePlayerTo(balloon.transform.position);
            yield return Physics();

            Assert.IsTrue(balloon == null, "Touching the balloon pops it.");
            Assert.AreEqual(0, _deaths, "And it doesn't hurt him.");
        }

        // ---- Clown ----

        private Clown MakeClown(out Rigidbody2D body, out SpriteRenderer sprite)
        {
            var go = new GameObject("Clown");
            go.SetActive(false);
            body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            sprite = go.AddComponent<SpriteRenderer>();
            var clown = go.AddComponent<Clown>();
            Set(clown, "pointA", Point("A", -5f, 0f));
            Set(clown, "pointB", Point("B", 5f, 0f));
            go.SetActive(true);
            return clown;
        }

        [UnityTest]
        public IEnumerator Clown_PatrolsTowardItsPoint_WhenNicholasIsFar()
        {
            MakeClown(out var body, out _);
            yield return Physics();

            Assert.AreEqual(1.5f, body.linearVelocity.x, 0.01f, "Walking to point B at patrol speed.");
        }

        [UnityTest]
        public IEnumerator Clown_ChasesNicholas_WhenHeComesClose_AndFacesHim()
        {
            MovePlayerTo(new Vector2(-3f, 0f)); // Inside the 5 chase range, the other way from point B.
            MakeClown(out var body, out var sprite);
            yield return Physics();

            Assert.AreEqual(-3f, body.linearVelocity.x, 0.01f, "Running at him at chase speed.");
            Assert.IsTrue(sprite.flipX, "Art faces right, so heading left flips it.");
        }

        // ---- Jack in the box ----

        [UnityTest]
        public IEnumerator JackInTheBox_IsSafeUntilNicholasIsNear_ThenSprings()
        {
            var box = new GameObject("Jack In The Box");
            box.SetActive(false);
            var jack = new GameObject("Jack").transform;
            jack.SetParent(box.transform, false);
            var jackCollider = jack.gameObject.AddComponent<BoxCollider2D>();
            jackCollider.isTrigger = true;
            var jackInTheBox = box.AddComponent<JackInTheBox>();
            Set(jackInTheBox, "jack", jack);
            Set(jackInTheBox, "springSpeed", 100f);
            box.SetActive(true);
            yield return null;

            Assert.IsFalse(jackCollider.enabled, "Hidden in the box, it can't hurt anyone.");
            Assert.AreEqual(0f, jack.localPosition.y, 0.001f);

            MovePlayerTo(new Vector2(1f, 0f)); // Inside the 1.5 trigger range.
            yield return new WaitForSeconds(0.1f);

            Assert.IsTrue(jackCollider.enabled, "Out of the box, it's dangerous.");
            Assert.AreEqual(2f, jack.localPosition.y, 0.01f, "Sprung up to its full height.");

            MovePlayerTo(new Vector2(100f, 100f));
            yield return new WaitForSeconds(0.7f);
            Assert.IsFalse(jackCollider.enabled, "Back in the box after a moment.");
            Assert.AreEqual(0f, jack.localPosition.y, 0.01f);
        }

        // ---- Thrown things: the clown's ball and the sword ----

        private static GameObject MakeThrown<T>(string name, out T thrown) where T : Component
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.AddComponent<Rigidbody2D>().gravityScale = 0f;
            go.AddComponent<CircleCollider2D>().isTrigger = true;
            thrown = go.AddComponent<T>();
            Set(thrown, "breaksOn", (LayerMask)(1 << GroundLayer));
            go.SetActive(true);
            return go;
        }

        [UnityTest]
        public IEnumerator ClownBall_CostsALife_AndBreaks()
        {
            var ball = MakeThrown<ClownBall>("Ball", out _);
            MovePlayerTo(ball.transform.position);
            yield return Physics();

            Assert.AreEqual(1, _deaths, "A hit costs Nicholas a life.");
            Assert.IsTrue(ball == null, "And the ball breaks.");
        }

        [UnityTest]
        public IEnumerator SwordProjectile_CostsALife_AndBreaks()
        {
            var sword = MakeThrown<SwordProjectile>("Sword", out _);
            MovePlayerTo(sword.transform.position);
            yield return Physics();

            Assert.AreEqual(1, _deaths, "A hit costs Nicholas a life.");
            Assert.IsTrue(sword == null, "And the sword breaks.");
        }

        [UnityTest]
        public IEnumerator ThrownThings_BreakOnTheGround_WithoutHurtingAnyone()
        {
            var ground = new GameObject("Ground") { layer = GroundLayer };
            ground.transform.position = new Vector3(0f, -5f, 0f);
            ground.AddComponent<BoxCollider2D>().size = new Vector2(20f, 1f);
            var ball = MakeThrown<ClownBall>("Ball", out _);
            ball.transform.position = new Vector3(-2f, -5f, 0f);
            var sword = MakeThrown<SwordProjectile>("Sword", out _);
            sword.transform.position = new Vector3(2f, -5f, 0f);
            Physics2D.SyncTransforms();
            yield return Physics();

            Assert.IsTrue(ball == null, "The ball breaks on the ground.");
            Assert.IsTrue(sword == null, "So does the sword.");
            Assert.AreEqual(0, _deaths);
        }

        [UnityTest]
        public IEnumerator ThrownThings_DisappearAfterTheirLifetime()
        {
            var ball = MakeThrown<ClownBall>("Ball", out var clownBall);
            Set(clownBall, "lifetime", 0.1f);
            var sword = MakeThrown<SwordProjectile>("Sword", out var swordProjectile);
            Set(swordProjectile, "lifetime", 0.1f);
            ball.transform.position = sword.transform.position = new Vector3(-20f, 20f, 0f);
            yield return new WaitForSeconds(0.3f);

            Assert.IsTrue(ball == null && sword == null, "Nothing flies forever.");
        }

        [UnityTest]
        public IEnumerator Sword_Launch_FliesAndPointsTheWayItIsThrown()
        {
            var sword = MakeThrown<SwordProjectile>("Sword", out var projectile);
            sword.transform.position = new Vector3(-20f, 20f, 0f);
            projectile.Launch(new Vector2(-8f, 0f));
            yield return Physics(1);

            Assert.AreEqual(-8f, sword.GetComponent<Rigidbody2D>().linearVelocity.x, 0.01f);
            Assert.AreEqual(180f, Mathf.Abs(Mathf.DeltaAngle(0f, sword.transform.eulerAngles.z)), 0.01f, "Point first, to the left.");
        }

        // ---- Sword Swallower and the brain that tells him when to throw ----

        private SwordSwallower MakeSwordSwallower(out SwordProjectile swordTemplate, bool withBrain = false)
        {
            // Active, so thrown copies are real bodies whose speed can be read; parked out of the way.
            var template = new GameObject("Sword Template");
            template.transform.position = new Vector3(-50f, 50f, 0f);
            template.AddComponent<Rigidbody2D>().gravityScale = 0f;
            swordTemplate = template.AddComponent<SwordProjectile>();

            var go = new GameObject("Sword Swallower");
            go.SetActive(false);
            go.AddComponent<Animator>();
            go.AddComponent<SpriteRenderer>();
            var swallower = go.AddComponent<SwordSwallower>();
            Set(swallower, "swordPrefab", swordTemplate);
            if (withBrain)
            {
                go.AddComponent<ThrowerBrain>();
            }

            go.SetActive(true);
            return swallower;
        }

        [UnityTest]
        public IEnumerator SwordSwallower_TurnsAndThrowsAtNicholas_ThenCoolsDown()
        {
            var swallower = MakeSwordSwallower(out _);
            SwordProjectile thrown = null;
            swallower.Thrown += sword => thrown = sword;
            yield return null;

            Assert.IsTrue(swallower.CanThrow);
            Assert.IsTrue(swallower.ThrowAt(new Vector2(5f, 0f)), "A throw starts.");
            Assert.IsTrue(swallower.FacingRight, "He turns to face Nicholas, to his right.");
            Assert.IsTrue(swallower.IsThrowing);
            Assert.IsFalse(swallower.ThrowAt(new Vector2(5f, 0f)), "No second throw mid-throw.");

            swallower.ReleaseSword(); // What the Throw clip's release frame calls.
            Assert.IsNotNull(thrown, "A sword leaves his hand.");
            Assert.AreEqual(new Vector2(10f, 0f), thrown.GetComponent<Rigidbody2D>().linearVelocity, "Level, toward Nicholas.");
            Assert.IsFalse(swallower.IsThrowing);
            Assert.IsFalse(swallower.CanThrow, "Cooling down before the next one.");
        }

        [UnityTest]
        public IEnumerator SwordSwallower_GivesUpAThrowThatNeverReleases()
        {
            var swallower = MakeSwordSwallower(out _);
            yield return null;
            swallower.ThrowAt(new Vector2(-5f, 0f));
            Assert.IsFalse(swallower.FacingRight, "Faces left, toward the target.");

            yield return new WaitForSeconds(2.2f); // Past the 2 s limit with no release frame.
            Assert.IsFalse(swallower.IsThrowing, "He shouldn't be stuck mid-throw forever.");
        }

        [UnityTest]
        public IEnumerator ThrowerBrain_ThrowsWhenNicholasIsInRange_AndNotOtherwise()
        {
            var swallower = MakeSwordSwallower(out _, withBrain: true);
            yield return null;
            yield return null;
            Assert.IsFalse(swallower.IsThrowing, "Nicholas is far away, so no throw.");

            MovePlayerTo(new Vector2(6f, 0f)); // Inside the 10 range.
            yield return null;
            yield return null;
            Assert.IsTrue(swallower.IsThrowing, "In range, he throws.");
        }
    }
}
