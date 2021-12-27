
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
#endif

namespace VirtualAviationJapan
{
    [RequireComponent(typeof(Camera))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class OneShotCamera : UdonSharpBehaviour
    {
        public RawImage overrideRawImage;
        private new Camera camera;

        private void Start()
        {
            camera = GetComponent<Camera>();
        }

        private void OnPostRender()
        {
            SendCustomEventDelayedFrames(nameof(_LatePostRender), 1);
        }

        public void _LatePostRender()
        {
            gameObject.SetActive(false);

            if (overrideRawImage != null)
            {
                overrideRawImage.texture = camera.targetTexture;
            }
        }
    }
}
