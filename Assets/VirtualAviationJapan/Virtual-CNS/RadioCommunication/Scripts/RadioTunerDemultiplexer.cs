using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;

namespace VirtualAviationJapan
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
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
        public void _StartPTT() => SelectedTuner._StartPTT();
        public void _EndPTT() => SelectedTuner._EndPTT();
        public void _Keypad1() => SelectedTuner._Keypad1();
        public void _Keypad2() => SelectedTuner._Keypad2();
        public void _Keypad3() => SelectedTuner._Keypad3();
        public void _Keypad4() => SelectedTuner._Keypad4();
        public void _Keypad5() => SelectedTuner._Keypad5();
        public void _Keypad6() => SelectedTuner._Keypad6();
        public void _Keypad7() => SelectedTuner._Keypad7();
        public void _Keypad8() => SelectedTuner._Keypad8();
        public void _Keypad9() => SelectedTuner._Keypad9();
        public void _Keypad0() => SelectedTuner._Keypad0();
        public void _KeypadClear() => SelectedTuner._KeypadClear();

        public void _TransferFrequency()
        {
            for (var i = 0; i < tuners.Length; i++)
            {
                var active = tuners[i];
                var standby = standbyTuners[i];
                active._TakeOwnership();
                standby._TakeOwnership();

                var tmp = active.Frequency;
                active.Frequency = standby.Frequency;
                standby.Frequency = tmp;

                active.RequestSerialization();
                standby.RequestSerialization();
            }
        }
    }
}
