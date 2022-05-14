
using TMPro;
using UdonRadioCommunication;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VirtualAviatioJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RadioTuner : UdonSharpBehaviour
    {
        [Header("COM")]
        public float defaultComFrequency = 118.0f;
        public float minFrequency = 118.0f;
        public float maxFrequency = 139.975f;
        public float comFrequencyStep = 0.025f;
        public bool hasStandbyFrequency = true;

        [Header("References")]
        public Receiver receiver;
        public Transmitter transmitter;
        public GameObject comIndicator, micIndicator;
        public TextMeshPro display;

        [UdonSynced][FieldChangeCallback(nameof(ComFrequency))] private float _comFrequency = 118.0f;
        public float ComFrequency
        {
            private set
            {
                var roundedValue = Mathf.Round(Mathf.Clamp(value, minFrequency, maxFrequency) / comFrequencyStep) * comFrequencyStep;

                if (receiver) receiver._SetFrequency(roundedValue);
                if (transmitter)
                {
                    if (transmitter.Active) transmitter._SetActive(false);
                    transmitter._SetFrequency(roundedValue);
                }

                _comFrequency = roundedValue;

                UpdateDisplay();
            }
            get => _comFrequency;
        }

        [UdonSynced][FieldChangeCallback(nameof(ComStandbyFrequency))] private float _comStandbyFrequency = 118.0f;

        public float ComStandbyFrequency
        {
            private set
            {
                var roundedValue = Mathf.Round(Mathf.Clamp(value, minFrequency, maxFrequency) / comFrequencyStep) * comFrequencyStep;

                _comStandbyFrequency = roundedValue;

                UpdateDisplay();
            }
            get => _comStandbyFrequency;
        }

        [UdonSynced][FieldChangeCallback(nameof(Com))] private bool _com;
        public bool Com
        {
            private set
            {
                if (receiver) receiver._SetActive(value);
                if (!value) Mic = false;
                _com = value;
            }
            get => _com;
        }

        [UdonSynced][FieldChangeCallback(nameof(Mic))] private bool _mic;
        public bool Mic
        {
            private set
            {
                if (transmitter && !value) transmitter._SetActive(value);
                if (micIndicator) micIndicator.SetActive(value);
                _mic = value;
            }
            get => _mic;
        }

        private string displayFormat;

        private void OnEnable()
        {
            if (receiver) receiver._SetActive(Com);
        }

        private void Start()
        {
            if (receiver)
            {
                receiver.sync = false;
                receiver.indicator = comIndicator;
                receiver.limitRange = false;
            }

            if (display) displayFormat = display.text;

            ComFrequency = ComStandbyFrequency = defaultComFrequency;
        }

        private void OnDisable()
        {
            if (receiver) receiver._SetActive(false);
            if (transmitter && Networking.IsOwner(transmitter.gameObject)) transmitter._SetActive(false);
        }

        private void UpdateDisplay()
        {
            display.text = string.Format(displayFormat, ComFrequency, hasStandbyFrequency ? (object)ComStandbyFrequency : (object)"INOP");
        }

        public void _TakeOwnership()
        {
            if (Networking.IsOwner(gameObject)) return;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public void _ToggleCom()
        {
            _TakeOwnership();
            Com = !Com;
            RequestSerialization();
        }

        public void _ToggleMic()
        {
            _TakeOwnership();
            Mic = !Mic;
            RequestSerialization();
        }

        public void _ToggleComAndMic()
        {
            _TakeOwnership();
            Com = Mic = !Com;
            RequestSerialization();
        }

        public void _SetComFrequency(float value)
        {
            _TakeOwnership();
            if (hasStandbyFrequency) ComStandbyFrequency = value;
            else ComFrequency = value;
            RequestSerialization();
        }
        private void AddComFrequency(float value)
        {
            _SetComFrequency((hasStandbyFrequency ? ComStandbyFrequency : ComFrequency) + value);
        }
        public void _IncrementComFrequency() => AddComFrequency(comFrequencyStep);
        public void _DecrementComFrequency() => AddComFrequency(-comFrequencyStep);
        public void _FastIncrementComFrequency() => AddComFrequency(1.0f);
        public void _FastDecrementComFrequency() => AddComFrequency(-1.0f);
        public void _TransferFrequency()
        {
            _TakeOwnership();
            var tmp = ComFrequency;
            ComFrequency = ComStandbyFrequency;
            ComStandbyFrequency = tmp;
            RequestSerialization();
        }

        public void _StartPTT()
        {
            if (transmitter) transmitter._SetActive(Mic);
        }
        public void _EndPTT()
        {
            if (transmitter) transmitter._SetActive(false);
        }
    }
}
