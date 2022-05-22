using System;
using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(AudioSource))]
    public class ATISPlayer : UdonSharpBehaviour
    {
        private AudioClip[] words = { };
        private int wordIndex;
        private AudioSource audioSource;

        private ATISGenerator _generator;
        public ATISGenerator Generator {
            private set {
                gameObject.SetActive(value);

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
            Generator = null;
        }

        private void Update()
        {
            if (!Generator)
            {
                gameObject.SetActive(false);
                return;
            }

            if (wordIndex > words.Length)
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

        public void _Play(ATISGenerator generator)
        {
            Generator = generator;
        }

        public void _Stop()
        {
            Generator = null;
        }
    }
}
