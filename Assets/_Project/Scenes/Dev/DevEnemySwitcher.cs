using UnityEngine;
using UnityEngine.InputSystem;

// Test only, not for the game. Shows one enemy at a time: press 1, 2, 3 and so on to swap to that
// enemy at its starting spot, or 0 to hide them all. H hides the key list in the top-right corner.
public class DevEnemySwitcher : MonoBehaviour
{
    [Tooltip("The enemies to swap between, in key order: the first is key 1.")]
    [SerializeField] private GameObject[] enemies;
    [Tooltip("Which one shows at the start: 1 for the first, 0 for none.")]
    [SerializeField] private int startWith = 1;

    private Vector3[] _starts;
    private int _current;
    private bool _showLabel = true;
    private GUIStyle _style;

    private static readonly Key[] NumberKeys =
    {
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9,
    };

    private void Awake()
    {
        _starts = new Vector3[enemies.Length];
        for (var i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
            {
                _starts[i] = enemies[i].transform.position;
            }
        }

        Show(startWith);
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        for (var i = 0; i < enemies.Length && i < NumberKeys.Length; i++)
        {
            if (keyboard[NumberKeys[i]].wasPressedThisFrame)
            {
                Show(i + 1);
            }
        }

        if (keyboard[Key.Digit0].wasPressedThisFrame)
        {
            Show(0);
        }

        if (keyboard[Key.H].wasPressedThisFrame)
        {
            _showLabel = !_showLabel;
        }
    }

    // Shows enemy number "number" (1 for the first) and hides the rest. 0 hides them all.
    private void Show(int number)
    {
        _current = number;
        for (var i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null)
            {
                continue;
            }

            var show = i + 1 == number;
            if (show)
            {
                // Turn it off and on again at its start spot, so it starts fresh.
                enemies[i].SetActive(false);
                enemies[i].transform.position = _starts[i];
            }

            enemies[i].SetActive(show);
        }
    }

    private void OnGUI()
    {
        if (!_showLabel)
        {
            return;
        }

        var text = "";
        for (var i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
            {
                text += $"{(i + 1 == _current ? "> " : "   ")}{i + 1}  {enemies[i].name}\n";
            }
        }

        text += "   0  none     H  hide this";

        _style ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft,
            normal = { textColor = Color.white },
        };

        var content = new GUIContent(text);
        var size = _style.CalcSize(content);
        const float pad = 14f;
        var box = new Rect(Screen.width - size.x - pad * 3, pad, size.x + pad * 2, size.y + pad * 2);

        // A dark panel behind the text so it reads on any part of the level.
        var old = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(box, Texture2D.whiteTexture);
        GUI.color = old;
        GUI.Label(new Rect(box.x + pad, box.y + pad, size.x, size.y), content, _style);
    }
}
