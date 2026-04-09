#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace VirtualCNS
{
    [CustomEditor(typeof(ATISGenerator))]
    public class ATISGeneratorEditor : Editor
    {
        private SerializedProperty template;
        private SerializedProperty windHeadings;
        private SerializedProperty runwayTemplates;
        private SerializedProperty defaultRunwayIndex;
        private SerializedProperty windTemplate;
        private SerializedProperty windWithGustTemplate;
        private SerializedProperty windSource;
        private SerializedProperty windVariableName;
        private SerializedProperty windGustVariableName;
        private SerializedProperty minWind;
        private SerializedProperty digits;
        private SerializedProperty phonetics;
        private SerializedProperty periodInterval;
        private SerializedProperty repeatInterval;
        private SerializedProperty clipWords;
        private SerializedProperty clips;

        private void OnEnable()
        {
            template = serializedObject.FindProperty(nameof(ATISGenerator.template));
            windHeadings = serializedObject.FindProperty(nameof(ATISGenerator.windHeadings));
            runwayTemplates = serializedObject.FindProperty(nameof(ATISGenerator.runwayTemplates));
            defaultRunwayIndex = serializedObject.FindProperty(nameof(ATISGenerator.defaultRunwayIndex));
            windTemplate = serializedObject.FindProperty(nameof(ATISGenerator.windTemplate));
            windWithGustTemplate = serializedObject.FindProperty(nameof(ATISGenerator.windWithGustTemplate));
            windSource = serializedObject.FindProperty(nameof(ATISGenerator.windSource));
            windVariableName = serializedObject.FindProperty(nameof(ATISGenerator.windVariableName));
            windGustVariableName = serializedObject.FindProperty(nameof(ATISGenerator.windGustVariableName));
            minWind = serializedObject.FindProperty(nameof(ATISGenerator.minWind));
            digits = serializedObject.FindProperty(nameof(ATISGenerator.digits));
            phonetics = serializedObject.FindProperty(nameof(ATISGenerator.phonetics));
            periodInterval = serializedObject.FindProperty(nameof(ATISGenerator.periodInterval));
            repeatInterval = serializedObject.FindProperty(nameof(ATISGenerator.repeatInterval));
            clipWords = serializedObject.FindProperty(nameof(ATISGenerator.clipWords));
            clips = serializedObject.FindProperty(nameof(ATISGenerator.clips));
        }

        public override void OnInspectorGUI()
        {
            UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target);

            serializedObject.Update();

            EditorGUILayout.PropertyField(template);
            EditorGUILayout.Space();

            DrawRunwayOperations();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Wind Source", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(windSource);
            EditorGUILayout.PropertyField(windVariableName);
            EditorGUILayout.PropertyField(windGustVariableName);
            EditorGUILayout.PropertyField(minWind);
            EditorGUILayout.PropertyField(windTemplate);
            EditorGUILayout.PropertyField(windWithGustTemplate);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audio Tables", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(digits, true);
            EditorGUILayout.PropertyField(phonetics, true);
            EditorGUILayout.PropertyField(periodInterval);
            EditorGUILayout.PropertyField(repeatInterval);

            EditorGUILayout.Space();
            DrawClipTable();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRunwayOperations()
        {
            EditorGUILayout.LabelField("Runway Operations", EditorStyles.boldLabel);

            SyncArraySizes(windHeadings, runwayTemplates);
            var count = Mathf.Max(0, EditorGUILayout.IntField("Operation Count", windHeadings.arraySize));
            ResizeArrays(count, windHeadings, runwayTemplates);

            if (count == 0)
            {
                EditorGUILayout.HelpBox("Add at least one runway operation.", MessageType.Warning);
                defaultRunwayIndex.intValue = 0;
                return;
            }

            defaultRunwayIndex.intValue = Mathf.Clamp(defaultRunwayIndex.intValue, 0, count - 1);
            defaultRunwayIndex.intValue = EditorGUILayout.IntSlider("Default Runway Index", defaultRunwayIndex.intValue, 0, count - 1);

            for (var i = 0; i < count; i++)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"Operation {i}", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(windHeadings.GetArrayElementAtIndex(i), new GUIContent("Wind Heading"));
                    EditorGUILayout.PropertyField(runwayTemplates.GetArrayElementAtIndex(i), new GUIContent("Runway Template"));
                }
            }
        }

        private void DrawClipTable()
        {
            EditorGUILayout.LabelField("Word Clips", EditorStyles.boldLabel);

            SyncArraySizes(clipWords, clips);
            var count = Mathf.Max(0, EditorGUILayout.IntField("Clip Count", clipWords.arraySize));
            ResizeArrays(count, clipWords, clips);

            for (var i = 0; i < count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(clipWords.GetArrayElementAtIndex(i), GUIContent.none);
                    EditorGUILayout.PropertyField(clips.GetArrayElementAtIndex(i), GUIContent.none);
                }
            }
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
