using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan
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
