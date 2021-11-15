using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [RequireComponent(typeof(SphereCollider))]
    public class MarkerReceiver : UdonSharpBehaviour
    {
        public AudioSource audioSource;
        public AudioClip innerMarker, middleMarker, outerMarker;

        private void Reset()
        {
            var sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            gameObject.layer = 18; // MirrorReflection
        }

        public void _InnerMarker() => Play(innerMarker);
        public void _MiddleMarker() => Play(middleMarker);
        public void _OuterMarker() => Play(outerMarker);

        private void Play(AudioClip sound)
        {
            if (sound == null || audioSource == null) return;
            audioSource.PlayOneShot(sound);
        }
    }
}
