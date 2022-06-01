using System;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using URC;

namespace VirtualAviationJapan
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(1200)] // After URC
    public class AirbandDatabase : UdonSharpBehaviour
    {
        [UdonSharpComponentInject] public UdonRadioCommunication urc;
        [ListView("Stations")] public float[] frequencies = { };
        [ListView("Stations")] public string[] identities = { };
        [ListView("Stations")] public ATISPlayer[] atisPlayers = { };
        private float frequencyStep;

        private void Start()
        {
            gameObject.name = nameof(AirbandDatabase);
            frequencyStep = urc.frequencyStep;

            var atisCount = 0;
            for (var i = 0; i < frequencies.Length; i++)
            {
                if (atisPlayers[i]) atisCount++;
            }

            var urcFrequencies = new float[urc.audioObjectFrequencies.Length + atisCount];
            var urcTemplates = new GameObject[urc.audioObjectTemplates.Length + atisCount];

            Array.Copy(urc.audioObjectFrequencies, urcFrequencies, urc.audioObjectFrequencies.Length);
            Array.Copy(urc.audioObjectTemplates, urcTemplates, urc.audioObjectTemplates.Length);

            var j = urc.audioObjectTemplates.Length;
            for (var i = 0; i < frequencies.Length; i++)
            {
                if (!atisPlayers[i]) continue;
                urcFrequencies[j] = frequencies[i];
                urcTemplates[j] = atisPlayers[i].gameObject;
                j++;
            }

            urc.audioObjectFrequencies = urcFrequencies;
            urc.audioObjectTemplates = urcTemplates;
        }

        public int _FindIndexByFrequency(float frequency)
        {
            for (var i = 0; i < frequencies.Length; i++)
            {
                if (Mathf.Abs(frequencies[i] - frequency) < frequencyStep / 2.0f) return i;
            }
            return -1;
        }

        public string _GetIdentityByFrequency(float frequency)
        {
            var index = _FindIndexByFrequency(frequency);
            if (index < 0) return null;
            return identities[index];
        }
    }
}
