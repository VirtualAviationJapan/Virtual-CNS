
using MonacaAirfrafts;
using TMPro;
using UdonRadioCommunication;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
using UnityEditor;
#endif

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RadioTuner : UdonSharpBehaviour
    {
        public const byte SELECTOR_MODE_COM = 1;
        public const byte SELECTOR_MODE_NAV = 2;

        public bool navMode = false;

        public float defaultFrequency = 118.0f;
        public float minFrequency = 118.0f;
        public float maxFrequency = 139.975f;
        public float frequencyStep = 0.025f;
        public string frequencyFormat = "000.000";

        [Header("References")]
        [HideIf("@navMode")] public Receiver receiver;
        [HideIf("@navMode")] public Transmitter transmitter;
        public GameObject listeningIndiator;
        [HideIf("@navMode")] public GameObject micIndicator;
        public TextMeshPro frequencyDisplay;
        [HideIf("@!navMode")] public TextMeshPro identityDisplay;
        [HideIf("@!navMode")] public NavSelector navSelector;
        [HideIf("@!navMode")] public IdentityPlayer identityPlayer;

        public string Identity
        {
            get
            {
                if (!navaidDatabase) return null;
                var index = navaidDatabase._FindIndexByFrequency(Frequency);
                if (index < 0) return null;
                return navaidDatabase.identities[index];
            }
        }

        [UdonSynced][FieldChangeCallback(nameof(Frequency))] private float _frequency = 118.0f;
        public float Frequency
        {
            set
            {
                var roundedValue = RoundFrequency(value);

                if (navMode)
                {
                    if (navSelector) navSelector._SetFrequency(value);
                    if (identityPlayer && Listen) identityPlayer._PlayIdentity(Identity);
                }
                else
                {
                    if (receiver) receiver._SetFrequency(roundedValue);
                    if (transmitter)
                    {
                        if (transmitter.Active) transmitter._SetActive(false);
                        transmitter._SetFrequency(roundedValue);
                    }
                }

                _frequency = roundedValue;

                UpdateDisplay();
            }
            get => _frequency;
        }

        [UdonSynced][FieldChangeCallback(nameof(Listen))] private bool _listen;
        public bool Listen
        {
            set
            {
                if (navMode)
                {
                    if (identityPlayer)
                    {
                        if (value) identityPlayer._PlayIdentity(Identity);
                        else identityPlayer._Stop();
                    }
                }
                else
                {
                    if (receiver) receiver._SetActive(value);
                }
                if (listeningIndiator) listeningIndiator.SetActive(value);

                if (!value) Mic = false;
                _listen = value;
            }
            get => _listen;
        }

        [UdonSynced][FieldChangeCallback(nameof(Mic))] private bool _mic;
        public bool Mic
        {
            set
            {
                if (transmitter && !value) transmitter._SetActive(value);
                if (micIndicator) micIndicator.SetActive(value);
                _mic = value;
            }
            get => _mic;
        }

        private NavaidDatabase navaidDatabase;
        private string input = null;

        private void OnEnable()
        {
            if (!navMode)
            {
                if (receiver) receiver._SetActive(Listen);
            }
        }

        private void Start()
        {
            if (!navMode)
            {
                if (receiver)
                {
                    receiver.sync = false;
                    receiver.indicator = listeningIndiator;
                    receiver.limitRange = false;
                }
            }

            var navaidObject = GameObject.Find(nameof(NavaidDatabase));
            if (navaidObject) navaidDatabase = navaidObject.GetComponent<NavaidDatabase>();

            Frequency = defaultFrequency;
            Mic = false;
            Listen = false;
        }

        private void OnDisable()
        {
            if (!navMode)
            {
                if (receiver) receiver._SetActive(false);
                if (transmitter && Networking.IsOwner(transmitter.gameObject)) transmitter._SetActive(false);
            }
        }

        private string ToFrequencyString()
        {
            var rawText = Frequency.ToString(frequencyFormat);
            if (input == null) return rawText;

            return $"{input}_";
        }

        private void UpdateDisplay()
        {
            if (frequencyDisplay) frequencyDisplay.text = ToFrequencyString();
            if (navMode)
            {
                if (identityDisplay) identityDisplay.text = Identity;
            }
        }

        private float RoundFrequency(float value)
        {
            return Mathf.Round(Mathf.Clamp(value, minFrequency, maxFrequency) / frequencyStep) * frequencyStep;
        }

        public void _TakeOwnership()
        {
            if (Networking.IsOwner(gameObject)) return;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public void _ToggleListen()
        {
            _TakeOwnership();
            Listen = !Listen;
            RequestSerialization();
        }

        public void _ToggleMic()
        {
            _TakeOwnership();
            Mic = !Mic;
            RequestSerialization();
        }

        public void _ToggleListenAndMic()
        {
            _TakeOwnership();
            Listen = Mic = !Listen;
            RequestSerialization();
        }

        public void _SetFrequency(float value)
        {
            _TakeOwnership();
            Frequency = value;
            RequestSerialization();
        }
        private void AddFrequency(float value)
        {
            _SetFrequency(Frequency + value);
        }
        public void _IncrementFrequency() => AddFrequency(frequencyStep);
        public void _DecrementFrequency() => AddFrequency(-frequencyStep);
        public void _FastIncrementFrequency() => AddFrequency(1.0f);
        public void _FastDecrementFrequency() => AddFrequency(-1.0f);

        public void _StartPTT()
        {
            if (!navMode && transmitter) transmitter._SetActive(Mic);
        }
        public void _EndPTT()
        {
            if (!navMode && transmitter) transmitter._SetActive(false);
        }

        public void _Keypad(int value)
        {

            var decimalPosition = frequencyFormat.IndexOf('.');

            if (input == null)
            {
                input = "";
            }
            input += (char)('0' + value);

            if (input.Length == decimalPosition) input += '.';

            if (input.Length >= frequencyFormat.Length)
            {
                _TakeOwnership();
                float parsedValue;
                if (float.TryParse(input, out parsedValue))
                {
                    Frequency = parsedValue;
                }
                input = null;
                RequestSerialization();
            }

            UpdateDisplay();
        }
        public void _Keypad1() => _Keypad(1);
        public void _Keypad2() => _Keypad(2);
        public void _Keypad3() => _Keypad(3);
        public void _Keypad4() => _Keypad(4);
        public void _Keypad5() => _Keypad(5);
        public void _Keypad6() => _Keypad(6);
        public void _Keypad7() => _Keypad(7);
        public void _Keypad8() => _Keypad(8);
        public void _Keypad9() => _Keypad(9);
        public void _Keypad0() => _Keypad(0);
        public void _KeypadClear()
        {
            if (input == null) return;
            input = input.Substring(0, input.Length - 1);
            if (input.EndsWith(".")) input = input.Substring(0, input.Length - 1);
            UpdateDisplay();
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        [Button("Preset Airband", true)]
        public void PresetAirband()
        {
            navMode = false;
            minFrequency = defaultFrequency = 118.0f;
            maxFrequency = 139.975f;
            frequencyStep = 0.025f;
            frequencyFormat = "000.000";
            this.ApplyProxyModifications();
        }

        [Button("Preset Navigation", true)]
        public void PresetVOR()
        {
            navMode = true;
            minFrequency = defaultFrequency = 108.00f;
            maxFrequency = 117.95f;
            frequencyStep = 0.05f;
            frequencyFormat = "000.00";
            this.ApplyProxyModifications();
        }
#endif
    }
}
