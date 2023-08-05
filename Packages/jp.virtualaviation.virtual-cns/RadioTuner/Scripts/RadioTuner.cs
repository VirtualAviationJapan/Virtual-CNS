
using TMPro;
using URC;
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
    [DefaultExecutionOrder(100)] // After NavSelector
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
        public bool autoMuteMic = true;
        public bool defaultMicMute = true;

        [Header("References")]
        public TextMeshPro frequencyDisplay;
        public GameObject listeningIndiator;
        [HideIf("@navMode")] public GameObject micIndicator;
        public TextMeshPro identityDisplay;
        [HideIf("@navMode")] public Receiver receiver;
        [HideIf("@navMode")] public Transmitter transmitter;
        [HideIf("@!navMode")] public NavSelector navSelector;
        [HideIf("@!navMode")] public IdentityPlayer identityPlayer;
        public Animator animator;
        [Popup("animatorBool", "@animator", "bool")] public string listenBool = "listen";
        [HideIf("@navMode")][Popup("animatorBool", "@animator", "bool")] public string micBool = "mic";

        public string Identity
        {
            get
            {
                if (navMode)
                {
                    if (!navaidDatabase) return null;
                    var index = navaidDatabase._FindIndexByFrequency(Frequency);
                    if (index < 0) return null;
                    return navaidDatabase.identities[index];
                }
                else
                {
                    if (!airbandDatabase) return null;
                    var index = airbandDatabase._FindIndexByFrequency(Frequency);
                    if (index < 0) return null;
                    return airbandDatabase.identities[index];
                }
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
                }
                else
                {
                    if (receiver) receiver.Frequency = roundedValue;
                    if (transmitter)
                    {
                        if (transmitter.Active) transmitter._SetActive(false);
                        transmitter._SetFrequency(roundedValue);
                    }
                }

                _frequency = roundedValue;

                if (navMode)
                {
                    if (identityPlayer && Listen) identityPlayer._PlayIdentity(Identity);
                }

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
                    if (receiver) receiver.Active = value;
                }
                if (listeningIndiator) listeningIndiator.SetActive(value);

                if (!value && autoMuteMic) Mic = false;

                if (animator) animator.SetBool(listenBool, value);

                _listen = value;
            }
            get => _listen;
        }

        private bool initialized;
        [UdonSynced][FieldChangeCallback(nameof(Mic))] private bool _mic;
        public bool Mic
        {
            set
            {
                // deactivate transmitter
                if (transmitter && !value) transmitter._SetActive(value);
                if (micIndicator) micIndicator.SetActive(value);

                if (animator) animator.SetBool(micBool, value);

                _mic = value;
            }
            get => _mic;
        }

        private NavaidDatabase navaidDatabase;
        private AirbandDatabase airbandDatabase;
        private char[] inputBuffer = null;
        private int inputCursor;

        private void OnEnable()
        {
            if (!initialized) return;

            // Debug.Log($"[Virtual-CNS][RadioTuner][{gameObject.GetInstanceID()}] Enabled");
            if (navMode)
            {
                if (identityPlayer && Listen) identityPlayer._PlayIdentity(Identity);
            }
            else
            {
                if (receiver) receiver.Active = Listen;
            }

            if (animator)
            {
                animator.SetBool(listenBool, Listen);
                if (!navMode) animator.SetBool(micBool, Mic);
            }
        }

        private void Start()
        {
            // Debug.Log($"[Virtual-CNS][RadioTuner][{gameObject.GetInstanceID()}] Initializing");
            if (!navMode)
            {
                if (receiver)
                {
                    receiver.sync = false;
                    receiver.indicator = listeningIndiator;
                    //receiver.limitRange = false;
                }

                if (transmitter)
                {
                    transmitter.statusIndicator = micIndicator;
                }
            }

            var navaidObject = GameObject.Find(nameof(NavaidDatabase));
            if (navaidObject) navaidDatabase = navaidObject.GetComponent<NavaidDatabase>();

            var airbandObject = GameObject.Find(nameof(AirbandDatabase));
            if (airbandObject) airbandDatabase = airbandObject.GetComponent<AirbandDatabase>();

            Frequency = defaultFrequency;
            Mic = !defaultMicMute;
            Listen = false;

            initialized = true;
            // Debug.Log($"[Virtual-CNS][RadioTuner][{gameObject.GetInstanceID()}] Initialized");
        }

        private void OnDisable()
        {
            // Debug.Log($"[Virtual-CNS][RadioTuner][{gameObject.GetInstanceID()}] Disabled");
            if (navMode)
            {
                if (identityPlayer) identityPlayer._Stop();
            }
            else
            {
                if (receiver) receiver.Active = false;
                if (transmitter && Networking.IsOwner(transmitter.gameObject)) transmitter._SetActive(false);
            }
        }

        private string ToFrequencyString()
        {
            var rawText = Frequency.ToString(frequencyFormat);
            if (inputBuffer == null) return rawText;

            return $"{new string(inputBuffer, 0, inputCursor)}_";
        }

        private void UpdateDisplay()
        {
            if (frequencyDisplay) frequencyDisplay.text = ToFrequencyString();
            if (identityDisplay) identityDisplay.text = Identity;
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

        public void _SetListen(bool value)
        {
            _TakeOwnership();
            Listen = value;
            RequestSerialization();
        }
        public void _Listen() => _SetListen(true);
        public void _Mute() => _SetListen(false);
        public void _ToggleListen() => _SetListen(!Listen);

        public void _SetMic(bool value)
        {
            _TakeOwnership();
            Mic = value;
            RequestSerialization();
        }
        public void _MicOn() => _SetMic(true);
        public void _MicMute() => _SetMic(false);
        public void _ToggleMic() => _SetMic(!Mic);


        public void _SetListenAndMic(bool value)
        {
            _TakeOwnership();
            Listen = Mic = value;
            RequestSerialization();
        }
        public void _ListenAndMicOn() => _SetListenAndMic(true);
        public void _MuteAll() => _SetListenAndMic(false);
        public void _ToggleListenAndMic() => _SetListenAndMic(!Listen);

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
            if (!navMode && transmitter) transmitter._Deactivate();
        }

        public void _Keypad(char value)
        {
            var decimalPosition = frequencyFormat.IndexOf('.');
            var formatLength = frequencyFormat.Length;

            if (inputBuffer == null)
            {
                inputCursor = 0;
                inputBuffer = frequencyFormat.ToCharArray();
                if (decimalPosition >= 0) inputBuffer[decimalPosition] = '.';
            }

            inputBuffer[inputCursor] = value;

            do
            {
                inputCursor++;
            }
            while (inputCursor < formatLength && !char.IsDigit(inputBuffer[inputCursor]));

            if (inputCursor >= formatLength)
            {
                _TakeOwnership();
                float parsedValue;
                if (float.TryParse(new string(inputBuffer), out parsedValue))
                {
                    Frequency = parsedValue;
                }
                inputBuffer = null;
                RequestSerialization();
            }

            UpdateDisplay();
        }
        public void _Keypad1() => _Keypad('1');
        public void _Keypad2() => _Keypad('2');
        public void _Keypad3() => _Keypad('3');
        public void _Keypad4() => _Keypad('4');
        public void _Keypad5() => _Keypad('5');
        public void _Keypad6() => _Keypad('6');
        public void _Keypad7() => _Keypad('7');
        public void _Keypad8() => _Keypad('8');
        public void _Keypad9() => _Keypad('9');
        public void _Keypad0() => _Keypad('0');
        public void _KeypadClear()
        {
            if (inputBuffer == null) return;
            do
            {
                inputCursor--;
            } while (inputCursor >= 0 && !char.IsDigit(inputBuffer[inputCursor]));

            if (inputCursor < 0) inputBuffer = null;

            UpdateDisplay();
        }

        public void _IncrementCourse()
        {
            if (navMode && navSelector) navSelector._IncrementCourse();
        }
        public void _DecrementCourse()
        {
            if (navMode && navSelector) navSelector._DecrementCourse();
        }
        public void _FastIncrementCourse()
        {
            if (navMode && navSelector) navSelector._FastIncrementCourse();
        }
        public void _FastDecrementCourse()
        {
            if (navMode && navSelector) navSelector._FastDecrementCourse();
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
        }

        [Button("Preset Navigation", true)]
        public void PresetVOR()
        {
            navMode = true;
            minFrequency = defaultFrequency = 108.00f;
            maxFrequency = 117.95f;
            frequencyStep = 0.05f;
            frequencyFormat = "000.00";
        }
#endif
    }
}
