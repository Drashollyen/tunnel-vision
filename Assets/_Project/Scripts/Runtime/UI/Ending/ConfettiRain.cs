using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    // Confetti made by code, so it needs no art: small coloured squares that fall, drift and spin
    // across this object, then start again at the top. Uses real time.
    // Put it on a full-screen object that doesn't block clicks.
    [RequireComponent(typeof(RectTransform))]
    public class ConfettiRain : MonoBehaviour
    {
        [SerializeField] private int pieces = 60;
        [SerializeField] private Vector2 pieceSize = new Vector2(14f, 8f);
        [SerializeField] private float fallSpeed = 160f;
        [SerializeField]
        private Color[] colours =
        {
            new Color(0.95f, 0.25f, 0.3f), new Color(1f, 0.85f, 0.3f), new Color(0.3f, 0.8f, 0.45f),
            new Color(0.35f, 0.6f, 1f), new Color(0.85f, 0.45f, 0.95f),
        };

        private sealed class Piece
        {
            public RectTransform Rect;
            public Image Image;
            public Color StartColour;
            public float Speed;
            public float Drift;
            public float Spin;
            public float Phase;
        }

        private readonly List<Piece> _pieces = new List<Piece>();
        private RectTransform _area;
        private Color _tint;
        private float _tintAmount;
        private float _tintSpeed;

        public int PieceCount => _pieces.Count;

        private void Awake()
        {
            _area = (RectTransform)transform;
            for (var i = 0; i < pieces; i++)
            {
                var go = new GameObject("Confetti Piece", typeof(RectTransform), typeof(Image));
                go.layer = gameObject.layer;
                var rect = (RectTransform)go.transform;
                rect.SetParent(_area, false);
                rect.sizeDelta = pieceSize;
                var image = go.GetComponent<Image>();
                image.color = colours.Length > 0 ? colours[i % colours.Length] : Color.white;
                image.raycastTarget = false;

                var piece = new Piece
                {
                    Rect = rect,
                    Image = image,
                    StartColour = image.color,
                    Speed = fallSpeed * Random.Range(0.6f, 1.4f),
                    Drift = Random.Range(20f, 60f),
                    Spin = Random.Range(-240f, 240f),
                    Phase = Random.Range(0f, Mathf.PI * 2f),
                };
                // Spread out to begin with, so it doesn't all start in one line.
                Place(piece, Random.Range(0f, 1f));
                _pieces.Add(piece);
            }
        }

        // Turns every piece to this colour over the given seconds.
        public void FadeTo(Color colour, float seconds)
        {
            _tint = colour;
            _tintAmount = 0f;
            _tintSpeed = seconds > 0f ? 1f / seconds : float.PositiveInfinity;
        }

        // Used by tests.
        public IEnumerable<Color> PieceColours
        {
            get
            {
                foreach (var piece in _pieces)
                {
                    yield return piece.Image.color;
                }
            }
        }

        private void Place(Piece piece, float heightFraction)
        {
            var size = _area.rect.size;
            piece.Rect.anchoredPosition = new Vector2(
                Random.Range(-size.x * 0.5f, size.x * 0.5f),
                Mathf.Lerp(-size.y * 0.5f, size.y * 0.5f, heightFraction) + pieceSize.y);
        }

        private void Update()
        {
            var dt = Time.unscaledDeltaTime;
            if (_tintSpeed > 0f && _tintAmount < 1f)
            {
                _tintAmount = Mathf.MoveTowards(_tintAmount, 1f, _tintSpeed * dt);
                foreach (var piece in _pieces)
                {
                    piece.Image.color = Color.Lerp(piece.StartColour, _tint, _tintAmount);
                }
            }

            var bottom = -_area.rect.height * 0.5f - pieceSize.y;
            foreach (var piece in _pieces)
            {
                var position = piece.Rect.anchoredPosition;
                position.y -= piece.Speed * dt;
                position.x += Mathf.Sin(Time.unscaledTime * 2f + piece.Phase) * piece.Drift * dt;
                piece.Rect.anchoredPosition = position;
                piece.Rect.Rotate(0f, 0f, piece.Spin * dt);

                if (position.y < bottom)
                {
                    Place(piece, 1f); // Back to the top.
                }
            }
        }
    }
}
