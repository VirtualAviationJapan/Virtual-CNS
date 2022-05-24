using UdonSharp;
using UnityEngine;
using UdonToolkit;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [RequireComponent(typeof(Collider))]
    public class MarkerBeacon : UdonSharpBehaviour
    {
        public readonly string[] MarkerTypes = new[] { "Inner", "Middle", "Outer" };

        public const int INNER_MARKER = 0;
        public const int MIDDLE_MARKER = 1;
        public const int OUTER_MARKER = 2;
        [Popup("@MarkerTypes")] public int type = 0;

        private void Reset()
        {
            var sphereCollider = GetComponent<Collider>();
            sphereCollider.isTrigger = true;
            gameObject.layer = 18; // MirrorReflection
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;

            foreach (var receiver in other.GetComponentsInChildren<MarkerReceiver>())
            {
                switch (type)
                {
                    case INNER_MARKER:
                        receiver._InnerMarker();
                        break;
                    case MIDDLE_MARKER:
                        receiver._MiddleMarker();
                        break;
                    case OUTER_MARKER:
                        receiver._OuterMarker();
                        break;
                }
            }
        }
    }
}
