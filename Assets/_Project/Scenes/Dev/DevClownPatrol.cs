using UnityEngine;

/// <summary>
/// Local test only, not for the game. Walks the clown under this object back and forth along its
/// platform using linecasts:
/// - an edge check, a short line down just ahead of his feet; no ground means turn around;
/// - a wall check at chest height; hitting ground there means turn around;
/// - a sight line, level at chest height, reaching ahead of and behind him; Nicholas crossing it
///   makes the clown laugh.
/// Each line is drawn in the Scene view (and the Game view with Gizmos on): green when it hits
/// something, yellow when it doesn't, red when the clown can see Nicholas. The sight line is
/// also drawn while not playing, so it can be lined up in the Scene view.
/// </summary>
public class DevClownPatrol : MonoBehaviour
{
    private static readonly int SpeedParam = Animator.StringToHash("Speed");

    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool startFacingRight;

    [Tooltip("How far he may walk either side of where he starts before turning back.")]
    [SerializeField] private float patrolDistance = 5f;

    [Header("Linecasts")]
    [SerializeField] private float edgeLookAhead = 0.6f;
    [SerializeField] private float edgeDepth = 0.5f;
    [SerializeField] private float wallCheckHeight = 1.5f;
    [Tooltip("How high above his feet the sight line runs.")]
    [SerializeField] private float sightHeight = 1.7f;
    [Tooltip("How far the sight line reaches in front of him.")]
    [SerializeField] private float sightAhead = 6f;
    [Tooltip("How far the sight line reaches behind him.")]
    [SerializeField] private float sightBehind = 6f;

    [Header("Laugh")]
    [SerializeField] private float laughCooldown = 4f;

    private Animator _animator;
    private SpriteRenderer _sprite;
    private TunnelVision.Enemies.ClownThrower _thrower;
    private float _direction;
    private float _startX;
    private float _nextLaugh;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _thrower = GetComponentInChildren<TunnelVision.Enemies.ClownThrower>();
        _direction = startFacingRight ? 1f : -1f;
        _startX = transform.position.x;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            return;
        }

        var facing = startFacingRight ? 1f : -1f;
        Vector2 feet = transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(feet + new Vector2(-facing * sightBehind, sightHeight),
                        feet + new Vector2(facing * sightAhead, sightHeight));
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(feet - new Vector2(patrolDistance, 0f), feet + new Vector2(patrolDistance, 0f));
    }

    private void Update()
    {
        Vector2 feet = transform.position;
        var nicholas = LookForNicholas(feet);

        // A Thrower Clown stops to throw while he can see Nicholas, and stands still mid-throw.
        if (_thrower != null && (nicholas != null || _thrower.IsThrowing))
        {
            _animator.SetFloat(SpeedParam, 0f);
            if (nicholas != null)
            {
                _direction = Mathf.Sign(nicholas.position.x - feet.x);
                _thrower.Face(_direction);
                _thrower.ThrowAt(nicholas);
            }

            return;
        }

        Walk(feet);
    }

    private void Walk(Vector2 feet)
    {
        // Edge check: a short line down just ahead of his feet.
        var edgeFrom = feet + new Vector2(_direction * edgeLookAhead, edgeDepth);
        var edgeTo = feet + new Vector2(_direction * edgeLookAhead, -edgeDepth);
        var groundAhead = Physics2D.Linecast(edgeFrom, edgeTo, groundLayer);
        Debug.DrawLine(edgeFrom, edgeTo, groundAhead ? Color.green : Color.yellow);

        // Wall check: a line forward at chest height.
        var wallFrom = feet + new Vector2(0f, wallCheckHeight);
        var wallTo = feet + new Vector2(_direction * (edgeLookAhead + 0.2f), wallCheckHeight);
        var wallAhead = Physics2D.Linecast(wallFrom, wallTo, groundLayer);
        Debug.DrawLine(wallFrom, wallTo, wallAhead ? Color.green : Color.yellow);

        // Patrol limit: turn back once he's walked patrolDistance from his start in this direction.
        var pastLimit = (feet.x - _startX) * _direction >= patrolDistance;
        Debug.DrawLine(new Vector2(_startX - patrolDistance, feet.y), new Vector2(_startX + patrolDistance, feet.y), Color.cyan);

        if (!groundAhead || wallAhead || pastLimit)
        {
            _direction = -_direction;
        }

        transform.position += new Vector3(_direction * walkSpeed * Time.deltaTime, 0f, 0f);

        if (_animator != null)
        {
            _animator.SetFloat(SpeedParam, walkSpeed);
        }

        if (_sprite != null)
        {
            if (_thrower != null)
                _thrower.Face(_direction);
            else
                _sprite.flipX = _direction < 0f;
        }
    }

    /// <summary>Casts the sight line and returns Nicholas if he's on it, otherwise null.</summary>
    private Transform LookForNicholas(Vector2 feet)
    {
        // A level line at chest height, reaching ahead of and behind him.
        var from = feet + new Vector2(-_direction * sightBehind, sightHeight);
        var to = feet + new Vector2(_direction * sightAhead, sightHeight);

        Transform seen = null;
        foreach (var hit in Physics2D.LinecastAll(from, to))
        {
            var body = hit.collider.attachedRigidbody;
            if (body != null && body.CompareTag("Player"))
            {
                seen = body.transform;
                break;
            }

            if (hit.collider.CompareTag("Player"))
            {
                seen = hit.collider.transform;
                break;
            }
        }

        Debug.DrawLine(from, to, seen != null ? Color.red : Color.yellow);

        if (seen != null && Time.time >= _nextLaugh)
        {
            _nextLaugh = Time.time + laughCooldown;
            BroadcastMessage("PlayLaugh", SendMessageOptions.DontRequireReceiver);
        }

        return seen;
    }
}
