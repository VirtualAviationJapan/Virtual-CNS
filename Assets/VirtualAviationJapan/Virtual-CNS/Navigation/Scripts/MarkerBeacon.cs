using UdonSharp;
using UnityEngine;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
#endif

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [RequireComponent(typeof(Collider))]
    public class MarkerBeacon : UdonSharpBehaviour
    {
        public const int INNER_MARKER = 0;
        public const int MIDDLE_MARKER = 1;
        public const int OUTER_MARKER = 2;
        public int type = 0;

        private void Reset()
        {
            var sphereCollider = GetComponent<Collider>();
            sphereCollider.isTrigger = true;
            gameObject.layer = 18; // MirrorReflection
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;

            var receiver = other.GetComponent<MarkerReceiver>();
            if (receiver == null) return;

            switch (type)
            {
                case INNER_MARKER:
                    receiver._InnerMarker();
                    break;
                case MIDDLE_MARKER:
                    receiver._MiddleMarker();
                    break;
                case OUTER_MARKER:
                    receiver._OuterMarker();
                    break;
            }
        }
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR

    [CustomEditor(typeof(MarkerBeacon))]
    public class MarkerBeaconEditor : Editor
    {
        private static readonly string[] MarkerTypes = new []{ "Inner", "Middle", "Outer" };
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var property = serializedObject.FindProperty(nameof(MarkerBeacon.type));
            property.intValue = EditorGUILayout.Popup("Type", property.intValue, MarkerTypes);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
