using TunnelVision.Enemies;
using UnityEngine;

/// <summary>
/// Local test only, not for the game. Casts a level sight line through the Sword Swallower under
/// this object at chest height; while Nicholas is on it, he throws his sword at him whenever he can.
/// The line is yellow, or red while Nicholas is on it.
/// </summary>
public class DevSwordSwallowerTest : MonoBehaviour
{
    [SerializeField] private float sightHeight = 1.8f;
    [SerializeField] private float sightRange = 12f;

    private SwordSwallower _swallower;

    private void Awake()
    {
        _swallower = GetComponentInChildren<SwordSwallower>();
    }

    private void OnDrawGizmos()
    {
        Vector2 feet = transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(feet + new Vector2(-sightRange, sightHeight), feet + new Vector2(sightRange, sightHeight));
    }

    private void Update()
    {
        Vector2 feet = transform.position;
        var from = feet + new Vector2(-sightRange, sightHeight);
        var to = feet + new Vector2(sightRange, sightHeight);

        Transform nicholas = null;
        foreach (var hit in Physics2D.LinecastAll(from, to))
        {
            var body = hit.collider.attachedRigidbody;
            if (body != null && body.CompareTag("Player"))
            {
                nicholas = body.transform;
                break;
            }
        }

        Debug.DrawLine(from, to, nicholas != null ? Color.red : Color.yellow);

        if (nicholas != null && _swallower != null)
        {
            _swallower.ThrowAt(nicholas);
        }
    }
}
