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
        public float frequency = 108.00f;
        public NavaidCapability capability;
        public Transform glideSlope;
        public bool hideFromMap;

        public bool IsNDB => (capability & NavaidCapability.NDB) != 0;
        public bool IsVOR => (capability & NavaidCapability.VOR) != 0;
        public bool IsILS => (capability & NavaidCapability.ILS) != 0;
        public bool HasDME => (capability & NavaidCapability.DME) != 0;
        public Transform DmeTransform {
            get {
                if (!HasDME) return null;
                if (IsILS && glideSlope != null) return glideSlope;
                return transform;
            }
        }

        private void Reset()
        {
            hideFlags = HideFlags.DontSaveInBuild;
        }

        private void OnValidate()
        {
            gameObject.name = identity;
        }
    }
}
