    using UdonSharp;
    using UnityEngine;
    using UdonToolkit;
namespace VirtualAviationJapan
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AirbandDatabase : UdonSharpBehaviour
    {
        public float frequencyStep = 0.025f;
        [ListView("Stations")] public float[] frequencies = { };
        [ListView("Stations")] public string[] identities = { };
        [ListView("Stations")] public ATISPlayer[] atisPlayers = { };
        private void Start()
        {
            gameObject.name = nameof(AirbandDatabase);
        }

        public int _FindIndexByFrequency(float frequency)
        {
            for (var i = 0; i < frequencies.Length; i++)
            {
                if (Mathf.Abs(frequencies[i] - frequency) < frequencyStep) return i;
            }
            return -1;
        }

        public bool _IsATIS(int index) => atisPlayers[index] != null;
    }
}
