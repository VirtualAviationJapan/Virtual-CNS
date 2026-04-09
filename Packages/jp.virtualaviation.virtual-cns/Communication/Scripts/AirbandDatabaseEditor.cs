#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace VirtualCNS
{
    [CustomEditor(typeof(AirbandDatabase))]
    public class AirbandDatabaseEditor : Editor
    {
        private SerializedProperty urc;
        private SerializedProperty frequencies;
        private SerializedProperty identities;
        private SerializedProperty atisPlayers;

        private void OnEnable()
        {
            urc = serializedObject.FindProperty(nameof(AirbandDatabase.urc));
            frequencies = serializedObject.FindProperty(nameof(AirbandDatabase.frequencies));
            identities = serializedObject.FindProperty(nameof(AirbandDatabase.identities));
            atisPlayers = serializedObject.FindProperty(nameof(AirbandDatabase.atisPlayers));
        }

        public override void OnInspectorGUI()
        {
            UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target);

            serializedObject.Update();

            EditorGUILayout.PropertyField(urc);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Airband Entries", EditorStyles.boldLabel);

            var count = DrawCountField("Entry Count", frequencies, identities, atisPlayers);

            for (var i = 0; i < count; i++)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"Entry {i}", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(frequencies.GetArrayElementAtIndex(i), new GUIContent("Frequency"));
                    EditorGUILayout.PropertyField(identities.GetArrayElementAtIndex(i), new GUIContent("Identity"));
                    EditorGUILayout.PropertyField(atisPlayers.GetArrayElementAtIndex(i), new GUIContent("ATIS Player"));
                }
            }

            EditorGUILayout.HelpBox("ATIS Player is optional. When set, its GameObject is registered to URC on Start.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        private static int DrawCountField(string label, params SerializedProperty[] properties)
        {
            var minSize = int.MaxValue;
            var maxSize = 0;
            for (var i = 0; i < properties.Length; i++)
            {
                minSize = Mathf.Min(minSize, properties[i].arraySize);
                maxSize = Mathf.Max(maxSize, properties[i].arraySize);
            }

            if (minSize == int.MaxValue) minSize = 0;

            if (minSize != maxSize)
            {
                EditorGUILayout.HelpBox($"{label} backing arrays are out of sync. Adjust the count to normalize them.", MessageType.Info);
            }

            var requestedSize = Mathf.Max(0, EditorGUILayout.IntField(label, maxSize));
            if (requestedSize != maxSize)
            {
                ResizeArrays(requestedSize, properties);
                return requestedSize;
            }

            return minSize;
        }

        private static void ResizeArrays(int size, params SerializedProperty[] properties)
        {
            for (var i = 0; i < properties.Length; i++)
            {
                properties[i].arraySize = size;
            }
        }
    }
}
#endif
