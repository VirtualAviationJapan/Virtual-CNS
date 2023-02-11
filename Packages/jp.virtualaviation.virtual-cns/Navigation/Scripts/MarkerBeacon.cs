using UdonSharp;
using UdonToolkit;
using UnityEngine;

namespace VirtualAviationJapan
{
    public enum MarkerBeaconType
    {
        Inner,
        Middle,
        Outer,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [RequireComponent(typeof(Collider))]
    public class MarkerBeacon : UdonSharpBehaviour
    {
        public MarkerBeaconType type;

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
                    case MarkerBeaconType.Inner:
                        receiver._InnerMarker();
                        break;
                    case MarkerBeaconType.Middle:
                        receiver._MiddleMarker();
                        break;
                    case MarkerBeaconType.Outer:
                        receiver._OuterMarker();
                        break;
                }
            }
        }
    }
}
