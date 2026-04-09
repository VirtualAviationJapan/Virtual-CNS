using System;
using UdonSharp;
using UnityEngine;

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(AudioSource))]
    public class ATISPlayer : UdonSharpBehaviour
    {
        private AudioClip[] words = { };
        private int wordIndex;
        private AudioSource audioSource;

        [SerializeField] private ATISGenerator _generator;
        public ATISGenerator Generator {
            set {
                if (value && value != _generator)
                {
                    words = value._Generate();
                    wordIndex = words != null && words.Length > 0
                        ? DateTime.UtcNow.Second % words.Length
                        : 0;
                }

                _generator = value;
            }
            get => _generator;
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (!Generator)
            {
                gameObject.SetActive(false);
                return;
            }

            if (words == null || words.Length == 0)
            {
                words = Generator._Generate();
                if (words == null || words.Length == 0)
                {
                    return;
                }
                wordIndex = Mathf.Clamp(wordIndex, 0, words.Length - 1);
            }

            if (wordIndex >= words.Length)
            {
                words = Generator._Generate();
                if (words == null || words.Length == 0)
                {
                    wordIndex = 0;
                    return;
                }
                wordIndex = 0;
            }

            if (!audioSource.isPlaying)
            {
                var clip = words[wordIndex];
                if (!clip)
                {
                    wordIndex += 1;
                    return;
                }

                audioSource.PlayOneShot(clip);
                wordIndex += 1;
            }
        }
    }
}
