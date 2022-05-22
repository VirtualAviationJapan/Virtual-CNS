using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VirtualAviationJapan
{
    public class Waypoint : MonoBehaviour
    {
        public string identity;
        public WaypointType type;

        private void Reset()
        {
            hideFlags = HideFlags.DontSaveInBuild;
            identity = gameObject.name;
        }

        private void OnValidate()
        {
            gameObject.name = identity;
        }

#if UNITY_EDITOR
#endif
    }

    public enum WaypointType
    {
        Waypoint,
        Aerodrome,
        WaypointRNAV,
    }

}
