#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace VirtualCNS
{
    internal static class RadioTunerSliceEditorGUI
    {
        public static void DrawTunerDetails(RadioTuner tuner, string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.ObjectField("Tuner", tuner, typeof(RadioTuner), true);
                if (!tuner)
                {
                    EditorGUILayout.HelpBox("Assign a RadioTuner to edit its linked references here.", MessageType.Info);
                    return;
                }

                var serializedTuner = new SerializedObject(tuner);
                serializedTuner.Update();

                var navMode = serializedTuner.FindProperty(nameof(RadioTuner.navMode));
                var frequencyDisplay = serializedTuner.FindProperty(nameof(RadioTuner.frequencyDisplay));
                var listeningIndiator = serializedTuner.FindProperty(nameof(RadioTuner.listeningIndiator));
                var micIndicator = serializedTuner.FindProperty(nameof(RadioTuner.micIndicator));
                var identityDisplay = serializedTuner.FindProperty(nameof(RadioTuner.identityDisplay));
                var receiver = serializedTuner.FindProperty(nameof(RadioTuner.receiver));
                var transmitter = serializedTuner.FindProperty(nameof(RadioTuner.transmitter));
                var navSelector = serializedTuner.FindProperty(nameof(RadioTuner.navSelector));
                var identityPlayer = serializedTuner.FindProperty(nameof(RadioTuner.identityPlayer));
                var animator = serializedTuner.FindProperty(nameof(RadioTuner.animator));
                var listenBool = serializedTuner.FindProperty(nameof(RadioTuner.listenBool));
                var micBool = serializedTuner.FindProperty(nameof(RadioTuner.micBool));

                EditorGUILayout.PropertyField(navMode);
                EditorGUILayout.PropertyField(frequencyDisplay);
                EditorGUILayout.PropertyField(listeningIndiator);
                EditorGUILayout.PropertyField(identityDisplay);

                if (navMode.boolValue)
                {
                    EditorGUILayout.PropertyField(navSelector);
                    EditorGUILayout.PropertyField(identityPlayer);
                }
                else
                {
                    EditorGUILayout.PropertyField(micIndicator);
                    EditorGUILayout.PropertyField(receiver);
                    EditorGUILayout.PropertyField(transmitter);
                }

                EditorGUILayout.PropertyField(animator);
                EditorGUILayout.PropertyField(listenBool);
                if (!navMode.boolValue)
                {
                    EditorGUILayout.PropertyField(micBool);
                }

                serializedTuner.ApplyModifiedProperties();
            }
        }

        public static void CopyTuners(SerializedProperty arrayProperty, RadioTuner[] tuners)
        {
            arrayProperty.arraySize = tuners?.Length ?? 0;
            for (var i = 0; i < arrayProperty.arraySize; i++)
            {
                arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue = tuners[i];
            }
        }
    }

    [CustomEditor(typeof(AudioSelector))]
    public class AudioSelectorEditor : Editor
    {
        private SerializedProperty comTuners;
        private SerializedProperty navTuners;
        private SerializedProperty markerReceiver;
        private SerializedProperty xmtrMode;

        private RadioTunerDemultiplexer comSource;
        private RadioTunerDemultiplexer navSource;

        private void OnEnable()
        {
            comTuners = serializedObject.FindProperty(nameof(AudioSelector.comTuners));
            navTuners = serializedObject.FindProperty(nameof(AudioSelector.navTuners));
            markerReceiver = serializedObject.FindProperty(nameof(AudioSelector.markerReceiver));
            xmtrMode = serializedObject.FindProperty(nameof(AudioSelector.xmtrMode));
        }

        public override void OnInspectorGUI()
        {
            UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target);

            serializedObject.Update();

            EditorGUILayout.PropertyField(markerReceiver);
            EditorGUILayout.PropertyField(xmtrMode);
            EditorGUILayout.Space();

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                comSource = EditorGUILayout.ObjectField("Load COMs", comSource, typeof(RadioTunerDemultiplexer), true) as RadioTunerDemultiplexer;
                if (change.changed && comSource)
                {
                    RadioTunerSliceEditorGUI.CopyTuners(comTuners, comSource.tuners);
                }
            }

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                navSource = EditorGUILayout.ObjectField("Load NAVs", navSource, typeof(RadioTunerDemultiplexer), true) as RadioTunerDemultiplexer;
                if (change.changed && navSource)
                {
                    RadioTunerSliceEditorGUI.CopyTuners(navTuners, navSource.tuners);
                }
            }

            EditorGUILayout.PropertyField(comTuners, true);
            EditorGUILayout.PropertyField(navTuners, true);

            var selector = (AudioSelector)target;
            for (var i = 0; i < selector.comTuners.Length; i++)
            {
                if (!selector.comTuners[i]) continue;
                EditorGUILayout.Space();
                RadioTunerSliceEditorGUI.DrawTunerDetails(selector.comTuners[i], $"COM {i + 1}");
            }

            for (var i = 0; i < selector.navTuners.Length; i++)
            {
                if (!selector.navTuners[i]) continue;
                EditorGUILayout.Space();
                RadioTunerSliceEditorGUI.DrawTunerDetails(selector.navTuners[i], $"NAV {i + 1}");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(StandbyFrequencySwitcher))]
    public class StandbyFrequencySwitcherEditor : Editor
    {
        private SerializedProperty active;
        private SerializedProperty standby;

        private void OnEnable()
        {
            active = serializedObject.FindProperty(nameof(StandbyFrequencySwitcher.active));
            standby = serializedObject.FindProperty(nameof(StandbyFrequencySwitcher.standby));
        }

        public override void OnInspectorGUI()
        {
            UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target);

            serializedObject.Update();
            EditorGUILayout.PropertyField(active);
            EditorGUILayout.PropertyField(standby);
            serializedObject.ApplyModifiedProperties();

            var switcher = (StandbyFrequencySwitcher)target;
            EditorGUILayout.Space();
            RadioTunerSliceEditorGUI.DrawTunerDetails(switcher.active, "Active");
            EditorGUILayout.Space();
            RadioTunerSliceEditorGUI.DrawTunerDetails(switcher.standby, "Standby");
        }
    }

    [CustomEditor(typeof(RadioTunerDemultiplexer))]
    public class RadioTunerDemultiplexerEditor : Editor
    {
        private SerializedProperty tuners;
        private SerializedProperty standbyTuners;
        private SerializedProperty toggledObjects;

        private void OnEnable()
        {
            tuners = serializedObject.FindProperty(nameof(RadioTunerDemultiplexer.tuners));
            standbyTuners = serializedObject.FindProperty(nameof(RadioTunerDemultiplexer.standbyTuners));
            toggledObjects = serializedObject.FindProperty(nameof(RadioTunerDemultiplexer.toggledObjects));
        }

        public override void OnInspectorGUI()
        {
            UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target);

            serializedObject.Update();
            EditorGUILayout.PropertyField(tuners, true);
            EditorGUILayout.PropertyField(standbyTuners, true);
            EditorGUILayout.PropertyField(toggledObjects, true);
            serializedObject.ApplyModifiedProperties();

            var demultiplexer = (RadioTunerDemultiplexer)target;
            var rowCount = Mathf.Max(demultiplexer.tuners.Length, demultiplexer.standbyTuners.Length, demultiplexer.toggledObjects.Length);
            for (var i = 0; i < rowCount; i++)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Tuner {i + 1}", EditorStyles.boldLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    if (i < demultiplexer.toggledObjects.Length)
                    {
                        EditorGUILayout.ObjectField("Toggle Target", demultiplexer.toggledObjects[i], typeof(GameObject), true);
                    }

                    if (i < demultiplexer.tuners.Length)
                    {
                        RadioTunerSliceEditorGUI.DrawTunerDetails(demultiplexer.tuners[i], "Active");
                    }

                    if (i < demultiplexer.standbyTuners.Length)
                    {
                        RadioTunerSliceEditorGUI.DrawTunerDetails(demultiplexer.standbyTuners[i], "Standby");
                    }
                }
            }
        }
    }
}
#endif
