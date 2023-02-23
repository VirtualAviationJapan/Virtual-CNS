using UnityEngine;
using TMPro;

namespace VirtualCNS
{
    [RequireComponent(typeof(TMP_Text))]
    public class Callsign : MonoBehaviour
    {
        public string Text
        {
            set => GetComponent<TMP_Text>().text = value;
            get => GetComponent<TMP_Text>().text;
        }

        private void Reset()
        {
            hideFlags = HideFlags.DontSaveInBuild;
        }
    }
}
