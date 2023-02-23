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
                    wordIndex = DateTime.UtcNow.Second % words.Length;
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

            if (wordIndex >= words.Length)
            {
                words = Generator._Generate();
                wordIndex = 0;
            }

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(words[wordIndex]);
                wordIndex += 1;
            }
        }
    }
}
