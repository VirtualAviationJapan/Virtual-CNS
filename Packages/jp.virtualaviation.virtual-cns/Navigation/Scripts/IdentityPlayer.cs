using System;
using UdonSharp;
using UnityEngine;

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(AudioSource))]
    public class IdentityPlayer : UdonSharpBehaviour
    {
        [SerializeField] private string identity;

        public AudioClip shortClip;
        public AudioClip longClip;
        public float characterInterval = 0.4f;
        public float wordInterval = 10.0f;

        private AudioSource audioSource;
        private readonly string[] letters = {
            "._", "_...", "_._.", "_..", ".", ".._.", "__.",
            "....", "..", ".___", "_._", "._..", "__", "_.",
            "___", ".__.", "__._", "._.", "...", "_", ".._",
            "..._", ".__", "__.._", "_.__", "__..",
        };
        private char[] encoded;
        private int index;
        private float lastPlayingTime;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            _Encode();
        }

        private void Update()
        {
            if (string.IsNullOrEmpty(identity) && !audioSource)
            {
                _Stop();
                return;
            }

            var time = Time.time;

            if (audioSource.isPlaying)
            {
                lastPlayingTime = time;
            }
            else
            {
                if (index < encoded.Length)
                {
                    var c = encoded[index];
                    switch (c)
                    {
                        case '.':
                            audioSource.PlayOneShot(shortClip);
                            index++;
                            break;
                        case '_':
                            audioSource.PlayOneShot(longClip);
                            index++;
                            break;
                        case ' ':
                            if (time >= lastPlayingTime + characterInterval) index++;
                            break;
                        default:
                            index++;
                            break;
                    }
                }
                else
                {
                    if (time >= lastPlayingTime + wordInterval) index = 0;
                }
            }

        }

        public void _SetIdentity(string value)
        {
            identity = value;
            _Encode();
        }

        public void _Encode()
        {
            if (string.IsNullOrEmpty(identity))
            {
                encoded = new char[0];
                return;
            }

            var characters = identity.ToUpper().ToCharArray();

            var tmp = new string[characters.Length];
            for (var i = 0; i < tmp.Length; i++)
            {
                var c = characters[i];
                if (char.IsLetter(c))
                {
                    tmp[i] = letters[c - 'A'];
                }
                else
                {
                    tmp[i] = "";
                }
            }
            encoded = string.Join(" ", tmp).ToCharArray();
        }

        public void _PlayIdentity(string value)
        {
            _SetIdentity(value);
            _Play();
        }

        public void _Play()
        {
            gameObject.SetActive(true);
        }

        public void _Stop()
        {
            if (audioSource) audioSource.Stop();
            gameObject.SetActive(false);
            identity = null;
        }
    }
}
