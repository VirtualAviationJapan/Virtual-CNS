using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(SphereCollider))]
    public class MarkerReceiver : UdonSharpBehaviour
    {
        public AudioSource audioSource;
        public AudioClip innerMarker, middleMarker, outerMarker;
        public Animator animator;
        public string triggerInnerMarker = "im", triggerMiddleMarker = "mm", triggerOuterMarker = "om", boolParameter = "mkr";

        public bool Mute
        {
            set
            {
                if (audioSource) audioSource.mute = value;
                if (animator) animator.SetBool(boolParameter, !value);
            }
            get => audioSource ? audioSource.mute : false;
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void Reset()
        {
            var sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            gameObject.layer = 18; // MirrorReflection
        }
#endif

        public void _InnerMarker() => Play(innerMarker, triggerInnerMarker);
        public void _MiddleMarker() => Play(middleMarker, triggerMiddleMarker);
        public void _OuterMarker() => Play(outerMarker, triggerOuterMarker);

        private void Play(AudioClip sound, string trigger)
        {
            if (sound && audioSource)
            {
                audioSource.PlayOneShot(sound);
            }

            if (animator)
            {
                animator.SetTrigger(trigger);
            }
        }
    }
}
