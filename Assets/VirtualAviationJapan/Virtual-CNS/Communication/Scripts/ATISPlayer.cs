
using System;
using System.Security.Permissions;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(AudioSource))]
    public class ATISPlayer : UdonSharpBehaviour
    {
        public const int STATE_PREFIX = 0;
        public const int STATE_PREFIX_PHONETIC = 1;
        public const int STATE_TIME = 2;
        public const int STATE_RUNWAY = 3;
        public const int STATE_WIND_WIND = 4;
        public const int STATE_WIND_DIRECTION = 5;
        public const int STATE_WIND_DEGREES = 6;
        public const int STATE_WIND_SPEED = 7;
        public const int STATE_WIND_KNOT = 8;
        public const int STATE_SUFFIX = 9;
        public const int STATE_SUFFIX_PHONETIC = 10;

        public const int STATE_START = STATE_PREFIX;
        public const int STATE_END = STATE_SUFFIX_PHONETIC;
        public const int PLAY_MODE_DIGITS = 1;
        public const int PLAY_MODE_NUMERIC = 2;

        public const float KNOTS = 1.944f;

        [TextArea] public string template = "<airport> information {0}, [{1}] Z. {2} approach. using runway [{3}]. {4}. visibility 10 kilometers, sky clear. Temperture [25], dewpoint [20], QNH [29.90]. advise you have information {0}";
        public string windTemplate = "wind [{0:000}] degrees {1} knots";
        public string windCalmTemplate = "wind calm";
        public string approach = "ILS [36]";
        public string usingRunway = "36";

        public UdonSharpBehaviour windSource;
        [Popup("programVariable", "@windSource", "vector")] public string windVariableName = "Wind";
        [Tooltip("Knots")] public float minWind = 5;

        public AudioClip[] digits = { }, phonetics = { };
        public AudioClip hundred, thousand;
        public AudioClip prefix, runway, wind, degrees, knot, suffix, decimal_, calm;
        [ListView("Words")] public string[] clipWords = { };
        [ListView("Words")] public AudioClip[] clips = { };
        private AudioSource audioSource;
        private int state;
        private char[] playingNumber;
        private int subState, subStateCount;
        private int index;
        private int playMode;
        private char[] playingDigits;
        private Vector3 windVector;
        private bool windCalm;
        private string timestamp;
        private string[] worlds;
        private float magneticDeclination;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();

            var navaidDatabaseObj = GameObject.Find("NavaidDatabase");
            if (navaidDatabaseObj) magneticDeclination = (float)((UdonBehaviour)navaidDatabaseObj.GetComponent(typeof(UdonBehaviour))).GetProgramVariable("magneticDeclination");

            UpdateInformation();
        }

        private void OnDisable()
        {
            if (audioSource) audioSource.Stop();
        }

        private void Update()
        {
            if (!audioSource) return;
            if (!audioSource.isPlaying) OnClipEnd();
        }

        private void Play()
        {
            UpdateInformation();
            SetState(STATE_START);
        }

        private void UpdateInformation()
        {
            var prevWind = windVector;
            windVector = windSource ? (Vector3)windSource.GetProgramVariable(windVariableName) : Vector3.zero;
            if (Vector3.Distance(windVector, prevWind) > 0)
            {
                index = (index + 1) % ('A' - 'Z');

                timestamp = DateTime.UtcNow.ToString("HHmm");
            }
            if (string.IsNullOrEmpty(timestamp)) timestamp = DateTime.UtcNow.ToString("HHmm");
            var windSpeed = windVector.magnitude * KNOTS;
            windCalm = windSpeed < minWind;

            // var windString = windCalm ? windCalmTemplate : string.Format(windTemplate, new object[] { windSpeed, GetHeading(-windVector.normalized) });
            // worlds = string.Format(template, 'A' + index, timestamp, approach, runway, windString).Split(' ');
        }

        // private void PlayWord(string word)
        // {
        //     var characters = word.ToCharArray();
        //     var firstCharacter = characters[0];
        //     var lastCharacter = characters[characters.Length - 1];

        //     if (lastCharacter == '.' || lastCharacter == ',') PlayWord(word.Substring(0, word.Length - 1));
        //     else if (word.Length == 1 && char.IsUpper(characters[0])) PlayPhonetic(firstCharacter);
        //     else if (firstCharacter == '[') PlayDigits(word.Substring(1, word.Length - 2));
        //     else if (char.IsDigit(firstCharacter)) PlayNumber(word);
        //     else PlayOneShot(GetWordClip(word));
        // }

        private AudioClip GetWordClip(string word)
        {
            for (var i = 0; i < clipWords.Length; i++)
            {
                if (clipWords[i] == word) return clips[i];
            }
            return null;
        }

        private void OnClipEnd()
        {
            SetSubState(subState + 1);
        }

        private void SetSubState(int value)
        {
            subState = value;
            if (value >= subStateCount)
            {
                SetState(state + 1);
                return;
            }

            switch (playMode)
            {
                case PLAY_MODE_DIGITS:
                    {
                        var c = playingDigits[value];
                        PlayOneShot(c == '.' ? decimal_ : digits[c - '0']);
                        break;
                    }
                case PLAY_MODE_NUMERIC:
                    {
                        var c = playingNumber[value];
                        if (char.IsDigit(c)) PlayOneShot(digits[c - '0']);
                        else if (c == ',') PlayOneShot(hundred);
                        else if (c == '.') PlayOneShot(thousand);
                        else if (char.IsUpper(c)) PlayOneShot(digits[c - 'A' + 20]);
                        else if (char.IsLower(c)) PlayOneShot(digits[c - 'a' + 10]);
                        break;
                    }
            }
        }

        private void SetState(int value)
        {
            if (value > STATE_END)
            {
                Play();
                return;
            }

            state = value;
            subStateCount = 1;

            switch (state)
            {
                case STATE_PREFIX:
                    PlayOneShot(prefix);
                    break;
                case STATE_PREFIX_PHONETIC:
                    PlayPhonetic((char)(index + 'A'));
                    break;
                case STATE_TIME:
                    PlayDigits(timestamp);
                    break;
                case STATE_RUNWAY:
                    PlayOneShot(runway);
                    break;
                case STATE_WIND_WIND:
                    PlayOneShot(wind);
                    break;
                case STATE_WIND_DIRECTION:
                    if (windCalm) PlayOneShot(calm);
                    else PlayWindDirection();
                    break;
                case STATE_WIND_DEGREES:
                    if (!windCalm) PlayOneShot(degrees);
                    break;
                case STATE_WIND_SPEED:
                    if (!windCalm) PlayNumeric(Mathf.RoundToInt(windVector.magnitude * KNOTS));
                    break;
                case STATE_WIND_KNOT:
                    if (!windCalm) PlayOneShot(knot);
                    break;
                case STATE_SUFFIX:
                    PlayOneShot(suffix);
                    break;
                case STATE_SUFFIX_PHONETIC:
                    PlayPhonetic((char)(index + 'A'));
                    break;
            }
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (!audioSource || !clip) return;
            audioSource.PlayOneShot(clip);
        }

        private void PlayPhonetic(char value)
        {
            PlayOneShot(phonetics[value - 'A']);
        }

        private void PlayDigits(string value)
        {
            Debug.Log($"Digits: {value}");
            playingDigits = value.ToCharArray();
            subStateCount = playingDigits.Length;
            playMode = PLAY_MODE_DIGITS;
            SetSubState(0);
        }

        private int GetHeading(Vector3 vector)
        {
            var heading = Mathf.RoundToInt(Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(vector, Vector3.up), Vector3.up) + 360) % 360;
            return heading == 0 ? 360 : heading;
        }

        private void PlayWindDirection()
        {
            var direction = Mathf.RoundToInt(Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(windVector, Vector3.up), Vector3.up) + magneticDeclination + 360) % 360;
            PlayDigits(direction == 0 ? "360" : direction.ToString("000"));
        }

        private void PlayNumeric(int value)
        {
            Debug.Log($"Numeric: {value}");
            var buf = "";
            while (value > 0)
            {
                if (value >= 1000)
                {
                    buf += value / 1000 + '.';
                    value %= 1000;
                }
                else if (value >= 100)
                {
                    buf += value / 100 + ',';
                    value %= 100;
                }
                else if (value >= 20)
                {
                    buf += 'A' + value / 10;
                    value %= 10;
                }
                else if (value >= 10)
                {
                    buf += 'a' + (value - 10);
                    value = -1;
                }
                else
                {
                    buf += value;
                    value = -1;
                }
            }
            playingNumber = buf.ToCharArray();
            subStateCount = playingNumber.Length;
            playMode = PLAY_MODE_NUMERIC;

            SetSubState(0);
        }
    }
}
