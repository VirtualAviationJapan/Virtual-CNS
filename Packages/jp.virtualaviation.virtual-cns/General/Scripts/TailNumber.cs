using UnityEngine;
using TMPro;

namespace VirtualAviationJapan
{
    [RequireComponent(typeof(TMP_Text))]
    public class TailNumber : MonoBehaviour
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
