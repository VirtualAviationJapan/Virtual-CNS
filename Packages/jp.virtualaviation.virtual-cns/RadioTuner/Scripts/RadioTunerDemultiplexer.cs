using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace VirtualCNS
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RadioTunerDemultiplexer : UdonSharpBehaviour
    {
        public RadioTuner[] tuners = { };
        public RadioTuner[] standbyTuners = { };
        public GameObject[] toggledObjects = { };

        [UdonSynced][FieldChangeCallback(nameof(Index))] private int _index;
        public int Index
        {
            set
            {
                if (tuners.Length == 0) return;
                var clamped = Mathf.Max(value, 0) % tuners.Length;

                if (_index < toggledObjects.Length && toggledObjects[_index]) toggledObjects[_index].SetActive(false);
                if (clamped < toggledObjects.Length && toggledObjects[clamped]) toggledObjects[clamped].SetActive(true);

                _index = clamped;
            }
            get => _index;
        }

        public RadioTuner SelectedTuner => Index < tuners.Length ? tuners[Index] : null;
        public RadioTuner StandbyTuner => Index < standbyTuners.Length ? standbyTuners[Index] : null;
        public RadioTuner FrequencyTargetTuner => StandbyTuner != null ? StandbyTuner : SelectedTuner;

        private void Start()
        {
            foreach (var o in toggledObjects)
            {
                if (!o) continue;
                o.SetActive(false);
            }
            if (tuners.Length > 0) Index = 0;
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

        public void _ToggleListen() { if (SelectedTuner != null) SelectedTuner._ToggleListen(); }
        public void _ToggleMic() { if (SelectedTuner != null) SelectedTuner._ToggleMic(); }
        public void _ToggleListenAndMic() { if (SelectedTuner != null) SelectedTuner._ToggleListenAndMic(); }
        public void _IncrementFrequency() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._IncrementFrequency(); }
        public void _DecrementFrequency() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._DecrementFrequency(); }
        public void _FastIncrementFrequency() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._FastIncrementFrequency(); }
        public void _FastDecrementFrequency() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._FastDecrementFrequency(); }

        public void _IncrementCourse() { if (SelectedTuner != null) SelectedTuner._IncrementCourse(); }
        public void _DecrementCourse() { if (SelectedTuner != null) SelectedTuner._DecrementCourse(); }
        public void _FastIncrementCourse() { if (SelectedTuner != null) SelectedTuner._FastIncrementCourse(); }
        public void _FastDecrementCourse() { if (SelectedTuner != null) SelectedTuner._FastDecrementCourse(); }

        public void _StartPTT() { if (SelectedTuner != null) SelectedTuner._StartPTT(); }
        public void _EndPTT() { if (SelectedTuner != null) SelectedTuner._EndPTT(); }
        public void _Keypad1() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._Keypad1(); }
        public void _Keypad2() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._Keypad2(); }
        public void _Keypad3() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._Keypad3(); }
        public void _Keypad4() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._Keypad4(); }
        public void _Keypad5() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._Keypad5(); }
        public void _Keypad6() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._Keypad6(); }
        public void _Keypad7() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._Keypad7(); }
        public void _Keypad8() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._Keypad8(); }
        public void _Keypad9() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._Keypad9(); }
        public void _Keypad0() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._Keypad0(); }
        public void _KeypadClear() { if (FrequencyTargetTuner != null) FrequencyTargetTuner._KeypadClear(); }

        public void _TransferFrequency()
        {
            if (SelectedTuner == null || StandbyTuner == null) return;
            if (SelectedTuner.navMode != StandbyTuner.navMode) return;
            SelectedTuner._TakeOwnership();
            StandbyTuner._TakeOwnership();

            var tmp = SelectedTuner.Frequency;
            SelectedTuner.Frequency = StandbyTuner.Frequency;
            StandbyTuner.Frequency = tmp;

            SelectedTuner.RequestSerialization();
            StandbyTuner.RequestSerialization();
        }
    }
}
