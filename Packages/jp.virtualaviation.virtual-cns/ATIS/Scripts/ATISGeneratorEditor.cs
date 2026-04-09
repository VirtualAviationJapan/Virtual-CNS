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

            var count = DrawCountField("Operation Count", 1, windHeadings, runwayTemplates);
            if (count == 0)
            {
                EditorGUILayout.HelpBox("Add at least one runway operation. ATIS generation is blocked until this is fixed.", MessageType.Error);
                defaultRunwayIndex.intValue = 0;
                if (GUILayout.Button("Create Default Runway Operation"))
                {
                    ResizeArrays(1, windHeadings, runwayTemplates);
                    windHeadings.GetArrayElementAtIndex(0).floatValue = 0.0f;
                    runwayTemplates.GetArrayElementAtIndex(0).stringValue = "runway operation unavailable";
                }
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

            var count = DrawCountField("Clip Count", 0, clipWords, clips);

            for (var i = 0; i < count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(clipWords.GetArrayElementAtIndex(i), GUIContent.none);
                    EditorGUILayout.PropertyField(clips.GetArrayElementAtIndex(i), GUIContent.none);
                }
            }
        }

        private static int DrawCountField(string label, int minimumSize, params SerializedProperty[] properties)
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

            var requestedSize = Mathf.Max(minimumSize, EditorGUILayout.IntField(label, Mathf.Max(minimumSize, maxSize)));
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
