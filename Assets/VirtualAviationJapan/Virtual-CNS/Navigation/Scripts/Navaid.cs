using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MonacaAirfrafts
{
    public class Navaid : MonoBehaviour
    {
        [Flags]
        public enum NavaidCapability
        {
            NDB = 1,
            VOR = 2,
            DME = 4,
            ILS = 8,
        }

        public string identity;
        public NavaidCapability capability;
        public Transform glideSlope;
        public bool hideFromMap;

        public bool IsNDB => (capability & NavaidCapability.NDB) != 0;
        public bool IsILS => (capability & NavaidCapability.ILS) != 0;
        public bool IsVOR => (capability & NavaidCapability.VOR) != 0;
        public Transform DmeTransform => IsILS ? glideSlope : transform;

        private void Reset()
        {
            hideFlags = HideFlags.DontSaveInBuild;
        }

        private void OnValidate()
        {
            gameObject.name = identity;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(Navaid))]
        [CanEditMultipleObjects]
        public class NavaidEditor : Editor
        {
        }
#endif
    }
}
