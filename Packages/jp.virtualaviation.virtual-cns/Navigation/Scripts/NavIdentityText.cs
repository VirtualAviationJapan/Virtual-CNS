using UdonSharp;
using UnityEngine;
using TMPro;

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(100)] // After NavSelector
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

            Debug.Log($"[Virtual-CNS][{this}:{GetHashCode():X8}] Initialized", gameObject);
        }

        public void _NavChanged()
        {
            if (text == null) return;

            text.text = string.Format(format, navSelector.Identity);
        }
    }
}
