
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon.Common.Enums;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
#endif

namespace VirtualCNS
{
    [RequireComponent(typeof(Camera))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class OneShotCamera : UdonSharpBehaviour
    {
        private new Camera camera;

        private void Start()
        {
            camera = GetComponent<Camera>();
            camera.enabled = true;
        }

        private void OnPostRender()
        {
            SendCustomEventDelayedSeconds(nameof(_LatePostRender), 1, EventTiming.LateUpdate);
        }

        public void _LatePostRender()
        {
            gameObject.SetActive(false);
        }
    }
}
