using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UniversalTextDriver : UdonSharpBehaviour
    {
        public Text[] texts = { };
        public TextMeshPro[] textMeshPros = { };
        public TextMeshProUGUI[] textMeshProUGUIs = { };

        private string _text;
        public string text {
            set {
                if (_text != null && _text == value) return;

                foreach (var t in texts) t.text = value;
                foreach (var t in textMeshPros) t.text = value;
                foreach (var t in textMeshProUGUIs) t.text = value;

                _text = value;
            }
            get => _text;
        }

        private void OnEnable()
        {
            enabled = false;
        }
    }
}
