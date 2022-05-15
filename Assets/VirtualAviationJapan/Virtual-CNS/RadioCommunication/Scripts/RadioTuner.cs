
using MonacaAirfrafts;
using TMPro;
using UdonRadioCommunication;
using UdonSharp;
using UnityEngine;
using VirtualAviationJapan;
using VRC.SDKBase;
using VRC.Udon;

namespace VirtualAviatioJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RadioTuner : UdonSharpBehaviour
    {
        public const byte SELECTOR_MODE_COM = 1;
        public const byte SELECTOR_MODE_NAV = 2;

        public bool hasStandbyFrequency = true;

        [Header("COM")]
        public float defaultComFrequency = 118.0f;
        public float minComFrequency = 118.0f;
        public float maxComFrequency = 139.975f;
        public float comFrequencyStep = 0.025f;

        [Header("Nav")]
        public float defaultNavFrequency = 108.00f;
        public float minNavFrequency = 108.00f;
        public float maxNavFrequency = 117.95f;
        public float navFrequencyStep = 0.05f;

        [Header("References")]
        public Receiver receiver;
        public Transmitter transmitter;
        public GameObject comMode;
        public GameObject comIndicator, micIndicator;
        public TextMeshPro comDisplay;
        public NavSelector navSelector;
        public IdentityPlayer identityPlayer;
        public GameObject navMode;
        public GameObject navIndicator;
        public TextMeshPro navDisplay;

        [UdonSynced][FieldChangeCallback(nameof(ComFrequency))] private float _comFrequency = 118.0f;
        public float ComFrequency
        {
            private set
            {
                var roundedValue = RoundComFrequency(value);

                if (receiver) receiver._SetFrequency(roundedValue);
                if (transmitter)
                {
                    if (transmitter.Active) transmitter._SetActive(false);
                    transmitter._SetFrequency(roundedValue);
                }

                _comFrequency = roundedValue;

                UpdateComDisplay();
            }
            get => _comFrequency;
        }

        [UdonSynced][FieldChangeCallback(nameof(ComStandbyFrequency))] private float _comStandbyFrequency = 118.0f;

        public float ComStandbyFrequency
        {
            private set
            {
                var roundedValue = RoundComFrequency(value);

                _comStandbyFrequency = roundedValue;

                UpdateComDisplay();
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

        [UdonSynced][FieldChangeCallback(nameof(NavFrequency))] private float _navFrequency = 118.0f;
        public float NavFrequency
        {
            private set
            {
                var roundedValue = RoundNavFrequency(value);

                if (navSelector)
                {
                    navSelector._SetFrequency(roundedValue);
                    if (identityPlayer) identityPlayer._SetIdentity(navSelector.Identity);
                }

                _navFrequency = roundedValue;

                UpdateNavDisplay();
            }
            get => _navFrequency;
        }

        [UdonSynced][FieldChangeCallback(nameof(NavStandbyFrequency))] private float _navStandbyFrequency = 118.0f;

        public float NavStandbyFrequency
        {
            private set
            {
                var roundedValue = RoundNavFrequency(value);

                _navStandbyFrequency = roundedValue;

                UpdateNavDisplay();
            }
            get => _navStandbyFrequency;
        }

        [UdonSynced][FieldChangeCallback(nameof(Nav))] private bool _nav;
        public bool Nav
        {
            private set
            {
                if (navIndicator) navIndicator.SetActive(value);

                if (identityPlayer && navSelector)
                {
                    if (value && navSelector) identityPlayer._PlayIdentity(navSelector.Identity);
                    else identityPlayer._Stop();
                }

                _nav = value;
            }
            get => _nav;
        }

        [UdonSynced][FieldChangeCallback(nameof(SelectorMode))] private byte _selectorMode;
        public byte SelectorMode
        {
            private set
            {
                if (comMode) comMode.SetActive(value == SELECTOR_MODE_COM);
                if (navMode) navMode.SetActive(value == SELECTOR_MODE_NAV);
                _selectorMode = value;
            }
            get => _selectorMode;
        }

        private string comDisplayFormat, navDisplayFormat;
        private NavaidDatabase navaidDatabase;

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

            if (comDisplay) comDisplayFormat = comDisplay.text;
            if (navDisplay) navDisplayFormat = navDisplay.text;

            var navaidObject = GameObject.Find(nameof(NavaidDatabase));
            if (navaidObject) navaidDatabase = navaidObject.GetComponent<NavaidDatabase>();

            ComFrequency = ComStandbyFrequency = defaultComFrequency;
            NavFrequency = NavStandbyFrequency = defaultNavFrequency;
            SelectorMode = SELECTOR_MODE_COM;
            Nav = false;
        }

        private void OnDisable()
        {
            if (receiver) receiver._SetActive(false);
            if (transmitter && Networking.IsOwner(transmitter.gameObject)) transmitter._SetActive(false);
        }

        private void UpdateComDisplay()
        {
            comDisplay.text = string.Format(comDisplayFormat, ComFrequency, hasStandbyFrequency ? (object)ComStandbyFrequency : (object)"INOP");
        }

        private void UpdateNavDisplay()
        {
            navDisplay.text = string.Format(navDisplayFormat, NavFrequency, hasStandbyFrequency ? (object)NavStandbyFrequency : (object)"INOP", navSelector ? navSelector.Identity : "   ");
        }

        private float RoundFrequency(float value, float min, float max, float step)
        {
            return Mathf.Round(Mathf.Clamp(value, min, max) / step) * step;
        }

        private float RoundComFrequency(float value)
        {
            return RoundFrequency(value, minComFrequency, maxComFrequency, comFrequencyStep);
        }

        private float RoundNavFrequency(float value)
        {
            return RoundFrequency(value, minNavFrequency, maxNavFrequency, navFrequencyStep);
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
        public void _SetNavFrequency(float value)
        {
            _TakeOwnership();
            if (hasStandbyFrequency) NavStandbyFrequency = value;
            else NavFrequency = value;
            RequestSerialization();
        }
        public void _SetFrequency(float value)
        {
            switch (SelectorMode)
            {
                case SELECTOR_MODE_COM:
                    _SetComFrequency(value); break;
                case SELECTOR_MODE_NAV:
                    _SetNavFrequency(value); break;
            }
        }
        private float GetFrequency()
        {
            switch (SelectorMode)
            {
                case SELECTOR_MODE_COM:
                    return hasStandbyFrequency ? ComStandbyFrequency : ComFrequency;
                case SELECTOR_MODE_NAV:
                    return hasStandbyFrequency ? NavStandbyFrequency : NavFrequency;
            }
            return 0;
        }
        private void AddFrequency(float value)
        {
            _SetFrequency(GetFrequency() + value);
        }
        private float GetFrequencyStep()
        {
            switch (SelectorMode)
            {
                case SELECTOR_MODE_COM:
                    return comFrequencyStep;
                case SELECTOR_MODE_NAV:
                    return navFrequencyStep;
            }
            return 0;
        }
        public void _IncrementFrequency() => AddFrequency(GetFrequencyStep());
        public void _DecrementFrequency() => AddFrequency(-GetFrequencyStep());
        public void _FastIncrementFrequency() => AddFrequency(1.0f);
        public void _FastDecrementFrequency() => AddFrequency(-1.0f);
        public void _TransferComFrequency()
        {
            _TakeOwnership();
            var tmp = ComFrequency;
            ComFrequency = ComStandbyFrequency;
            ComStandbyFrequency = tmp;
            RequestSerialization();
        }

        public void _ToggleNav()
        {
            _TakeOwnership();
            Nav = !Nav;
            RequestSerialization();
        }
        public void _TransferNavFrequency()
        {
            _TakeOwnership();
            var tmp = NavFrequency;
            NavFrequency = NavStandbyFrequency;
            NavStandbyFrequency = tmp;
            RequestSerialization();
        }
        public void _TransferFrequency()
        {
            switch (SelectorMode)
            {
                case SELECTOR_MODE_COM:
                    _TransferComFrequency(); break;
                case SELECTOR_MODE_NAV:
                    _TransferNavFrequency(); break;
            }
        }

        public void _StartPTT()
        {
            if (transmitter) transmitter._SetActive(Mic);
        }
        public void _EndPTT()
        {
            if (transmitter) transmitter._SetActive(false);
        }

        public void _SetMode(byte mode)
        {
            _TakeOwnership();
            SelectorMode = mode;
            RequestSerialization();
        }
        public void _ComMode() => _SetMode(SELECTOR_MODE_COM);
        public void _NavMode() => _SetMode(SELECTOR_MODE_NAV);
        public void _ToggleMode() => _SetMode(SelectorMode == SELECTOR_MODE_COM ? SELECTOR_MODE_NAV : SELECTOR_MODE_COM);
    }
}
