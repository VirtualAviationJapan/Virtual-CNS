using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(SphereCollider))]
    public class MarkerReceiver : UdonSharpBehaviour
    {
        [Header("Play Audio")]
        public AudioSource audioSource;
        public AudioClip innerMarker, middleMarker, outerMarker;

        [Header("Animator Trigger")]
        public Animator animator;
        [Popup("animatorTrigger", "@animator")] public string triggerInnerMarker = "im", triggerMiddleMarker = "mm", triggerOuterMarker = "om";
        [Popup("animatorBool", "@animator")] public string boolParameter = "mkr";

        [Header("Write Shader Property")]
        public new Renderer renderer;
        [MaterialOf(nameof(renderer))] public Material material;
        [Popup("shader", "@material", "vector")] public string materialPropertyName = "_Marker";

        [Header("Indicator")]
        public GameObject indicator;

        [UdonSynced][FieldChangeCallback(nameof(Mute))] private bool _mute;
        private int materialIndex;
        private MaterialPropertyBlock materialProperties;

        public bool Mute
        {
            private set
            {
                if (audioSource) audioSource.mute = value;
                if (animator) animator.SetBool(boolParameter, !value);
                if (indicator) indicator.SetActive(!value);
                _mute = value;
            }
            get => _mute;
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void Reset()
        {
            var sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            gameObject.layer = 18; // MirrorReflection
        }
#endif
        private void Start()
        {
            Mute = false;

            if (renderer)
            {
                materialIndex = MaterialIndexOf(renderer, material);
                if (materialIndex >= 0) materialProperties = new MaterialPropertyBlock();
            }
            _ClearMaterialProperties();
        }

        private void OnDisable()
        {
            _ClearMaterialProperties();
        }

        private int MaterialIndexOf(Renderer renderer, Material material)
        {
            for (var i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i] == material)
                {
                    return i;
                }
            }
            return -1;
        }

        public void _InnerMarker() => Play(innerMarker, triggerInnerMarker, Vector3.forward);
        public void _MiddleMarker() => Play(middleMarker, triggerMiddleMarker, Vector3.up);
        public void _OuterMarker() => Play(outerMarker, triggerOuterMarker, Vector3.right);

        public void _TakeOwnership()
        {
            if (Networking.IsOwner(gameObject)) return;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public void _SetMute(bool value)
        {
            _TakeOwnership();
            Mute = value;
            RequestSerialization();
        }

        private void Play(AudioClip sound, string trigger, Vector3 shaderValue)
        {
            if (sound && audioSource)
            {
                audioSource.PlayOneShot(sound);
            }

            if (animator)
            {
                animator.SetTrigger(trigger);
            }

            if (materialProperties != null)
            {
                SetMaterialProperty(shaderValue);
                SendCustomEventDelayedSeconds(nameof(_ClearMaterialProperties), sound ? sound.length : 10);
            }
        }

        private void SetMaterialProperty(Vector3 value)
        {
            if (materialProperties == null) return;
            renderer.GetPropertyBlock(materialProperties, materialIndex);
            materialProperties.SetVector(materialPropertyName, value);
            renderer.SetPropertyBlock(materialProperties, materialIndex);
        }

        public void _ClearMaterialProperties()
        {
            SetMaterialProperty(Vector3.zero);
        }
    }
}
