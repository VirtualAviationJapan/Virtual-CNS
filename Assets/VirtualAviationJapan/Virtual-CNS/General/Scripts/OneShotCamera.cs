
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using UdonToolkit;
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
            camera.enabled = true;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            camera.enabled = false;

            if (overrideRawImage != null)
            {
                overrideRawImage.texture = camera.targetTexture;
            }
        }


#if !COMPILER_UDONSHARP && UNITY_EDITOR
        [Button("New Render Texture", true)]
        public void NewRenderTexture()
        {
            GetComponent<Camera>().targetTexture = new RenderTexture(128, 128, 24, RenderTextureFormat.Depth);
        }

        private void Reset()
        {
            GetComponent<Camera>().enabled = false;
        }
#endif
    }

}
