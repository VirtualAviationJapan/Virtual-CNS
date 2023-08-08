using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using TMPro;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
using System.Linq;
#endif

namespace VirtualCNS
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [OnAfterEditor(nameof(RadioTunerDemultiplexer.OnAfterEditor))]
#endif
    public class RadioTunerDemultiplexer : UdonSharpBehaviour
    {
        [ListView("Tuners")] public RadioTuner[] tuners = { };
        [ListView("Tuners")] public RadioTuner[] standbyTuners = { };
        [ListView("Tuners")] public GameObject[] toggledObjects = { };

        [UdonSynced][FieldChangeCallback(nameof(Index))] private int _index;
        public int Index
        {
            set
            {
                var clamped = Mathf.Max(value, 0) % tuners.Length;

                if (toggledObjects[_index]) toggledObjects[_index].SetActive(false);
                if (toggledObjects[clamped]) toggledObjects[clamped].SetActive(true);

                _index = clamped;
            }
            get => _index;
        }

        public RadioTuner SelectedTuner => tuners[Index];
        public RadioTuner StandbyTuner => standbyTuners[Index];
        public RadioTuner FrequencyTargetTuner => StandbyTuner ? StandbyTuner : SelectedTuner;

        private void Start()
        {
            foreach (var o in toggledObjects)
            {
                if (!o) continue;
                o.SetActive(false);
            }
            Index = 0;
        }

        public void _TakeOwnership()
        {
            if (Networking.IsOwner(gameObject)) return;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public void _IncrementIndex()
        {
            _TakeOwnership();
            Index++;
            RequestSerialization();
        }

        public void _ToggleListen() => SelectedTuner._ToggleListen();
        public void _ToggleMic() => SelectedTuner._ToggleMic();
        public void _ToggleListenAndMic() => SelectedTuner._ToggleListenAndMic();
        public void _IncrementFrequency() => FrequencyTargetTuner._IncrementFrequency();
        public void _DecrementFrequency() => FrequencyTargetTuner._DecrementFrequency();
        public void _FastIncrementFrequency() => FrequencyTargetTuner._FastIncrementFrequency();
        public void _FastDecrementFrequency() => FrequencyTargetTuner._FastDecrementFrequency();

        public void _IncrementCourse() => SelectedTuner._IncrementCourse();
        public void _DecrementCourse() => SelectedTuner._DecrementCourse();
        public void _FastIncrementCourse() => SelectedTuner._FastIncrementCourse();
        public void _FastDecrementCourse() => SelectedTuner._FastDecrementCourse();

        public void _StartPTT() => SelectedTuner._StartPTT();
        public void _EndPTT() => SelectedTuner._EndPTT();
        public void _Keypad1() => FrequencyTargetTuner._Keypad1();
        public void _Keypad2() => FrequencyTargetTuner._Keypad2();
        public void _Keypad3() => FrequencyTargetTuner._Keypad3();
        public void _Keypad4() => FrequencyTargetTuner._Keypad4();
        public void _Keypad5() => FrequencyTargetTuner._Keypad5();
        public void _Keypad6() => FrequencyTargetTuner._Keypad6();
        public void _Keypad7() => FrequencyTargetTuner._Keypad7();
        public void _Keypad8() => FrequencyTargetTuner._Keypad8();
        public void _Keypad9() => FrequencyTargetTuner._Keypad9();
        public void _Keypad0() => FrequencyTargetTuner._Keypad0();
        public void _KeypadClear() => FrequencyTargetTuner._KeypadClear();

        public void _TransferFrequency()
        {
            SelectedTuner._TakeOwnership();
            StandbyTuner._TakeOwnership();

            var tmp = SelectedTuner.Frequency;
            SelectedTuner.Frequency = StandbyTuner.Frequency;
            StandbyTuner.Frequency = tmp;

            SelectedTuner.RequestSerialization();
            StandbyTuner.RequestSerialization();
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public static void OnAfterEditor(SerializedObject serializedObject)
        {
            var demultiplexer = serializedObject.targetObject as RadioTunerDemultiplexer;
            int i = 1;
            foreach (var (active, standby) in demultiplexer.tuners.Zip(demultiplexer.standbyTuners, (active, standby) => (active, standby)))
            {
                EditorGUILayout.LabelField($"Tuner {i++}");
                using (new EditorGUI.IndentLevelScope())
                {
                    StandbyFrequencySwitcher.SetupHelperGUI(active, standby);
                }
            }
        }
#endif
    }
}
