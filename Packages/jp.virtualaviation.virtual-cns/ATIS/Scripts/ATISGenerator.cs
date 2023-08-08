
using System;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.Udon;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ATISGenerator : UdonSharpBehaviour
    {
        public const int MAX_WORDS = 256;
        public const float KNOTS = 1.944f;
        readonly private char[] trimChars = new[] { '[', ']', ',', '.', ' ' };

        [TextArea] public string template = "AERODROME information [{0}] [{1}] [Z]. {2}. wind {3}. visibility 10 kilometers, sky clear. temperature [25], dewpoint [20], qnh [2992]. advise you have information [{0}]";
        [ListView("Runway Operations")] public float[] windHeadings = { 0.0f, 180.0f };
        [ListView("Runway Operations")]
        public string[] runwayTemplates = {
            "ils runway [36] approach. using runway [36]",
            "ils runway [18] approach. using runway [18]",
        };
        public int defaultRunwayIndex = 0;
        public string windTemplate = "[{0:000}] degrees {1:0} knots";
        public string windWithGustTemplate = "[{0:000}] degrees {1:0} knots, maximum {2:0} knots";

        public UdonSharpBehaviour windSource;
        [Popup("programVariable", "@windSource", "vector")] public string windVariableName = "Wind";
        [Popup("programVariable", "@windSource", "float")] public string windGustVariableName = "WindGustStrength";
        [Tooltip("Knots")] public float minWind = 0.5f;

        public AudioClip[] digits = { }, phonetics = { };
        public AudioClip periodInterval, repeatInterval;
        [ListView("Vocabulary")] public string[] clipWords = { };
        [ListView("Vocabulary")] public AudioClip[] clips = { };

        private float magneticDeclination;
        private int informationIndex;
        private AudioClip[] words;

        private void Start()
        {
            var navaidDatabaseObj = GameObject.Find("NavaidDatabase");
            if (navaidDatabaseObj) magneticDeclination = (float)((UdonBehaviour)navaidDatabaseObj.GetComponent(typeof(UdonBehaviour))).GetProgramVariable("magneticDeclination");
        }

        public AudioClip[] _Generate()
        {
            var now = DateTime.UtcNow;
            var hour = now.Hour;
            var minute = now.Minute / 30 * 30;

            var prevInformationIndex = informationIndex;
            informationIndex = (hour * 60 + minute) / 30 % ('Z' - 'A');
            if (words != null && prevInformationIndex == informationIndex) return words;

            var timestamp = string.Format("{0:00}{1:00}", hour, minute);

            var windVector = windSource ? (Vector3)windSource.GetProgramVariable(windVariableName) : Vector3.zero;
            var windGustStrength = windSource ? (float)windSource.GetProgramVariable(windGustVariableName) : 0.0f;

            var windSpeed = Mathf.RoundToInt(windVector.magnitude * KNOTS);
            var windCalm = windSpeed < minWind;
            var windHeading = Mathf.RoundToInt(Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(windVector, Vector3.up), Vector3.up) + magneticDeclination + 540) % 360;
            var gusty = windGustStrength > minWind;
            var maxWindSpeed = windSpeed + Mathf.RoundToInt(windGustStrength * KNOTS);

            var windString = windCalm ? "calm" : string.Format(gusty ? windWithGustTemplate : windTemplate, new object[] { windHeading, windSpeed, maxWindSpeed });
            var runwayOperationIndex = windCalm ? defaultRunwayIndex : IndexOfRunwayOperation(windHeading);

            var rawText = string.Format(template, (char)('A' + informationIndex), timestamp, runwayTemplates[runwayOperationIndex], windString);
            Debug.Log($"[Virtual-CNS][ATIS] Encoding: {rawText}");
            var rawWords = rawText.Split(' ');
            var wordsBuf = new AudioClip[MAX_WORDS];
            var wordsBufIndex = 0;
            var period = false;
            foreach (var rawWord in rawWords)
            {
                var word = rawWord.Trim(trimChars);
                var chars = word.ToCharArray();
                var firstChar = chars[0];

                if (period)
                {
                    wordsBuf[wordsBufIndex++] = periodInterval;
                }
                period = rawWord.EndsWith(".");

                if (rawWord.StartsWith("["))
                {
                    if (char.IsDigit(firstChar))
                    {
                        foreach (var c in chars)
                        {
                            wordsBuf[wordsBufIndex++] = GetDigitClip(c - '0');
                        }
                        continue;
                    }

                    if (char.IsUpper(firstChar))
                    {
                        wordsBuf[wordsBufIndex++] = GetPhoneticClip(firstChar);
                        continue;
                    }
                }

                if (char.IsDigit(firstChar))
                {
                    int value;
                    if (int.TryParse(word, out value))
                    {
                        while (value >= 0)
                        {
                            if (value >= 1000)
                            {
                                wordsBuf[wordsBufIndex++] = GetDigitClip(value / 1000);
                                wordsBuf[wordsBufIndex++] = GetWordClip("thousand");
                                value %= 1000;
                            }
                            else if (value >= 100)
                            {
                                wordsBuf[wordsBufIndex++] = GetDigitClip(value / 100);
                                wordsBuf[wordsBufIndex++] = GetWordClip("hundred");
                                value %= 100;
                            }
                            else if (value >= 20)
                            {
                                wordsBuf[wordsBufIndex++] = GetDigitClip(value / 10 * 10);
                                value %= 10;
                            }
                            else
                            {
                                wordsBuf[wordsBufIndex++] = GetDigitClip(value);
                                value = -1;
                            }
                        }
                    }
                    continue;
                }

                var clip = GetWordClip(word);
                if (clip)
                {
                    wordsBuf[wordsBufIndex++] = clip;
                    continue;
                }

                Debug.LogWarning($"[Virtual-CNS][ATIS] Failed to find clip of word \"{rawWord}\" (\"{word}\").");
            }
            wordsBuf[wordsBufIndex++] = repeatInterval;

            words = new AudioClip[wordsBufIndex];
            Array.Copy(wordsBuf, words, words.Length);

            return words;
        }

        private AudioClip GetDigitClip(int value)
        {
            if (value > 90 || value < 0)
            {
                Debug.LogError("[Virtual-CNS][ATIS] Wrong Digit: {value}");
                return null;
            }
            if (value >= 20) return digits[value / 10 + 18];
            if (value >= 10) return digits[value];
            return digits[value];
        }

        private AudioClip GetPhoneticClip(char c)
        {
            return phonetics[c - 'A'];
        }

        private int IndexOfRunwayOperation(float windHeading)
        {
            var minDifference = float.MaxValue;
            var minIndex = 0;

            for (var i = 0; i < windHeadings.Length; i++)
            {
                var difference = Mathf.Abs(Mathf.DeltaAngle(windHeading, windHeadings[i]));
                if (difference < minDifference)
                {
                    minDifference = difference;
                    minIndex = i;
                }
            }

            return minIndex;
        }

        private AudioClip GetWordClip(string word)
        {
            for (var i = 0; i < clipWords.Length; i++)
            {
                if (clipWords[i] == word) return clips[i];
            }
            return null;
        }
    }
}
