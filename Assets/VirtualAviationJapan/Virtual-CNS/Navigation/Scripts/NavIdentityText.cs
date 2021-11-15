using UdonSharp;
using UnityEngine;
using TMPro;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [DefaultExecutionOrder(1000)] // After NavSelector
    public class NavIdentityText : UdonSharpBehaviour
    {
        public TextMeshPro text;
        private string format;
        private NavSelector navSelector;
        private void Start()
        {
            if (text != null) format = text.text;

            navSelector = GetComponentInParent<NavSelector>();
            navSelector._Subscribe(this);
        }

        public void _NavChanged()
        {
            if (text == null) return;

            text.text = string.Format(format, navSelector.Identity);
        }
    }
}
