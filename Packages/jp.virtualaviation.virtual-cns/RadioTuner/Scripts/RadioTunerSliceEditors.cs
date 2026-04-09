#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace VirtualCNS
{
    internal static class RadioTunerSliceEditorGUI
    {
        public static void DrawTunerDetails(RadioTuner tuner, string label, bool? expectedNavMode = null)
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

                DrawRoleField(tuner, navMode, expectedNavMode);
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

        private static void DrawRoleField(RadioTuner tuner, SerializedProperty navMode, bool? expectedNavMode)
        {
            var role = navMode.boolValue ? "NAV" : "COM";

            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.Toggle("Navigation Mode", navMode.boolValue);
            }
            EditorGUILayout.LabelField("Role", role);

            if (!expectedNavMode.HasValue) return;

            var expectedRole = expectedNavMode.Value ? "NAV" : "COM";
            if (navMode.boolValue == expectedNavMode.Value)
            {
                EditorGUILayout.HelpBox($"Role matches expected {expectedRole} tuner setup.", MessageType.None);
                return;
            }

            EditorGUILayout.HelpBox($"This tuner is configured as {role}, but this slot expects {expectedRole}.", MessageType.Error);
            if (GUILayout.Button($"Apply {expectedRole} Preset"))
            {
                Undo.RecordObject(tuner, $"Preset {expectedRole} tuner");
                if (expectedNavMode.Value) tuner.PresetVOR();
                else tuner.PresetAirband();
                EditorUtility.SetDirty(tuner);
                navMode.serializedObject.Update();
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

        public static void DrawTunerArray(SerializedProperty arrayProperty, string label, bool expectedNavMode)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            var count = Mathf.Max(0, EditorGUILayout.IntField($"{label} Count", arrayProperty.arraySize));
            arrayProperty.arraySize = count;

            for (var i = 0; i < count; i++)
            {
                var tunerProperty = arrayProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(tunerProperty, new GUIContent($"{label} {i + 1}"));

                var tuner = tunerProperty.objectReferenceValue as RadioTuner;
                if (!tuner)
                {
                    EditorGUILayout.HelpBox("Assign a RadioTuner. Null slots create runtime no-op paths.", MessageType.Warning);
                    continue;
                }

                if (tuner.navMode != expectedNavMode)
                {
                    EditorGUILayout.HelpBox($"Assigned tuner role is {(tuner.navMode ? "NAV" : "COM")} but slot expects {(expectedNavMode ? "NAV" : "COM")}.", MessageType.Error);
                }
            }
        }

        public static void SyncArraySizes(int size, params SerializedProperty[] arrays)
        {
            for (var i = 0; i < arrays.Length; i++)
            {
                arrays[i].arraySize = size;
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

            RadioTunerSliceEditorGUI.DrawTunerArray(comTuners, "COM", false);
            EditorGUILayout.Space();
            RadioTunerSliceEditorGUI.DrawTunerArray(navTuners, "NAV", true);

            var selector = (AudioSelector)target;
            for (var i = 0; i < selector.comTuners.Length; i++)
            {
                if (!selector.comTuners[i]) continue;
                EditorGUILayout.Space();
                RadioTunerSliceEditorGUI.DrawTunerDetails(selector.comTuners[i], $"COM {i + 1}", false);
            }

            for (var i = 0; i < selector.navTuners.Length; i++)
            {
                if (!selector.navTuners[i]) continue;
                EditorGUILayout.Space();
                RadioTunerSliceEditorGUI.DrawTunerDetails(selector.navTuners[i], $"NAV {i + 1}", true);
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
            var activeTuner = active.objectReferenceValue as RadioTuner;
            var standbyTuner = standby.objectReferenceValue as RadioTuner;
            if (activeTuner && standbyTuner && activeTuner.navMode != standbyTuner.navMode)
            {
                EditorGUILayout.HelpBox("Active and standby tuners must have the same role. Frequency transfer is guarded against mixed COM/NAV pairs.", MessageType.Error);
            }
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            RadioTunerSliceEditorGUI.DrawTunerDetails(activeTuner, "Active", activeTuner ? activeTuner.navMode : (bool?)null);
            EditorGUILayout.Space();
            RadioTunerSliceEditorGUI.DrawTunerDetails(standbyTuner, "Standby", activeTuner ? activeTuner.navMode : (bool?)null);
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
            var rowCount = Mathf.Max(tuners.arraySize, standbyTuners.arraySize, toggledObjects.arraySize);
            rowCount = Mathf.Max(0, EditorGUILayout.IntField("Tuner Count", rowCount));
            RadioTunerSliceEditorGUI.SyncArraySizes(rowCount, tuners, standbyTuners, toggledObjects);

            for (var i = 0; i < rowCount; i++)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"Tuner {i + 1}", EditorStyles.boldLabel);

                    var toggleProperty = toggledObjects.GetArrayElementAtIndex(i);
                    var activeProperty = tuners.GetArrayElementAtIndex(i);
                    var standbyProperty = standbyTuners.GetArrayElementAtIndex(i);
                    var activeTuner = activeProperty.objectReferenceValue as RadioTuner;
                    var standbyTuner = standbyProperty.objectReferenceValue as RadioTuner;

                    EditorGUILayout.PropertyField(toggleProperty, new GUIContent("Toggle Target"));
                    EditorGUILayout.PropertyField(activeProperty, new GUIContent("Active Tuner"));
                    EditorGUILayout.PropertyField(standbyProperty, new GUIContent("Standby Tuner"));

                    if (!activeTuner)
                    {
                        EditorGUILayout.HelpBox("Active tuner is required for this slot.", MessageType.Warning);
                    }
                    else if (standbyTuner && activeTuner.navMode != standbyTuner.navMode)
                    {
                        EditorGUILayout.HelpBox("Standby tuner role does not match the active tuner role.", MessageType.Error);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();

            var demultiplexer = (RadioTunerDemultiplexer)target;
            for (var i = 0; i < demultiplexer.tuners.Length; i++)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Tuner {i + 1}", EditorStyles.boldLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    if (i < demultiplexer.toggledObjects.Length)
                    {
                        EditorGUILayout.ObjectField("Toggle Target", demultiplexer.toggledObjects[i], typeof(GameObject), true);
                    }

                    RadioTunerSliceEditorGUI.DrawTunerDetails(demultiplexer.tuners[i], "Active", demultiplexer.tuners[i] ? demultiplexer.tuners[i].navMode : (bool?)null);

                    if (i < demultiplexer.standbyTuners.Length)
                    {
                        RadioTunerSliceEditorGUI.DrawTunerDetails(demultiplexer.standbyTuners[i], "Standby", demultiplexer.tuners[i] ? demultiplexer.tuners[i].navMode : (bool?)null);
                    }
                }
            }
        }
    }
}
#endif
