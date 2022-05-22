
using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan
{
    [RequireComponent(typeof(Camera))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SetReplacementShader : UdonSharpBehaviour
    {
        public Shader shader;
        public string replacementTag;

        public bool onStart = true;
        private void OnStart()
        {
            if (onStart) _Trigger();
        }

        public void _Trigger()
        {
            var camera = GetComponent<Camera>();
            camera.SetReplacementShader(shader, replacementTag);
        }
    }
}
