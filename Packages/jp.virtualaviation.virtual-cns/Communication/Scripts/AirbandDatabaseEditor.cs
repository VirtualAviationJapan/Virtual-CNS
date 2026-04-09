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

            SyncArraySizes(frequencies, identities, atisPlayers);
            var count = Mathf.Max(0, EditorGUILayout.IntField("Entry Count", frequencies.arraySize));
            ResizeArrays(count, frequencies, identities, atisPlayers);

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

        private static void SyncArraySizes(params SerializedProperty[] properties)
        {
            var size = 0;
            for (var i = 0; i < properties.Length; i++)
            {
                size = Mathf.Max(size, properties[i].arraySize);
            }

            ResizeArrays(size, properties);
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
