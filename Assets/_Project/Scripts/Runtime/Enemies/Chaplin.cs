using System.Collections;
using UnityEngine;

namespace TunnelVision.Enemies
{
    public class Chaplin : MonoBehaviour
    {

        private BoxCollider2D bc;
        private SpriteRenderer currSprite;
        private bool swordStarted = false;
        public Sprite swordSprite;

        [SerializeField] private AudioSource ihyn;
        [SerializeField] private AudioSource swordaudio;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            bc = GetComponent<BoxCollider2D>();
            currSprite = GetComponent<SpriteRenderer>();

        }

        // Update is called once per frame
        void Update()
        {
            bool isTouchingAnything = bc.IsTouchingLayers();
            if (isTouchingAnything && !swordStarted)
            {
                swordStarted = true;
                StartCoroutine(PlayIHYN());
            }
        }

        // I hate you, Nicholas
        IEnumerator PlayIHYN()
        {
            // Play first audio then wait until it is finished before continuing
            ihyn.Play(0);
            yield return new WaitWhile(() => ihyn.isPlaying);
            SwallowSword();
        }

        private void SwallowSword()
        {
            // Play second audio and switch sprite to sword swallowing
            swordaudio.Play(0);
            currSprite.sprite = swordSprite;
        }
    }
}
