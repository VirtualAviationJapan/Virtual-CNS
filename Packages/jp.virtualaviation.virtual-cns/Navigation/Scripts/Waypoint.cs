using UdonSharp;
using UnityEngine;

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Waypoint : UdonSharpBehaviour
    {
        public string identity;
        public WaypointType type;

        private void OnValidate()
        {
            gameObject.name = identity;
        }
    }

    public enum WaypointType
    {
        Waypoint,
        Aerodrome,
        WaypointRNAV,
    }

}
