
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MonacaAirfrafts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(1000)] // After NavaidDatabase
    public class NavigationMap : UdonSharpBehaviour
    {
        public const uint NAVAID_NDB = 1;
        public const uint NAVAID_VOR = 2;
        public const uint NAVAID_DME = 4;
        public const uint NAVAID_ILS = 8;

        public const uint WAYPOINT_WAYPOINT = 0;
        public const uint WAYPOINT_AERDROME = 1;
        public const uint WAYPOINT_WAYPOINT_RNAV = 2;

        public float magneticDeclination = 0.0f;
        public float uiRadius = 810.0f;
        [Tooltip("NM")] public float initialRange = 10.0f;
        public GameObject vorTemplate, vordmeTemplate, waypointTemplate, aerodromeTemplate, waypointRNAVTemplate;
        public int updateInterval = 9;

        private Transform[] navaidMarkers, waypointMarkers;
        private NavaidDatabase database;
        private float scale;

        #region Unity Events
        private void Start()
        {
            gameObject.SetActive(false);

            var databaseObject = GameObject.Find(nameof(NavaidDatabase));
            if (databaseObject == null) return;

            database = databaseObject.GetComponent<NavaidDatabase>();
            if (database == null) return;

            navaidMarkers = InstantiateMarkers(database.identities, database.capabilities, database.hideFromMaps, true);
            waypointMarkers = InstantiateMarkers(database.waypointIdentities, database.waypointTypes, null, false);

            SetRange(initialRange);

            gameObject.SetActive(true);

            var navaidDatabaseObj = GameObject.Find("NavaidDatabase");
            if (navaidDatabaseObj) magneticDeclination = (float)((UdonBehaviour)navaidDatabaseObj.GetComponent(typeof(UdonBehaviour))).GetProgramVariable("magneticDeclination");

            Debug.Log($"[Virtual-CNS][{this}:{GetHashCode():X8}] Initialized", gameObject);

#if UNITY_ANDROID
            updateInterval *= 2;
#endif
        }

        private int updateIntervalOffset;
        private void OnEnable()
        {
            updateIntervalOffset = UnityEngine.Random.Range(0, updateInterval);
        }

        private void Update()
        {
            if ((Time.frameCount + updateIntervalOffset) % updateInterval != 0) return;
            UpdateTransform(scale);
        }

        private void SetRange(float value)
        {
            scale = uiRadius / (value * 1852.0f);
            InitializeMarkerPositions(navaidMarkers, database.transforms, scale);
            InitializeMarkerPositions(waypointMarkers, database.waypointTransforms, scale);
            UpdateTransform(scale);
        }
        #endregion
        private GameObject GetNavaidTemplate(uint capability)
        {
            if ((capability & NAVAID_ILS) != 0) return null;
            if ((capability & NAVAID_VOR) != 0)
            {
                if ((capability & NAVAID_DME) != 0) return vordmeTemplate;
                return vorTemplate;
            }
            return null;
        }

        private GameObject GetWaypointTemplate(uint type)
        {
            switch (type)
            {
                case WAYPOINT_WAYPOINT:
                    return waypointTemplate;
                case WAYPOINT_AERDROME:
                    return aerodromeTemplate;
                case WAYPOINT_WAYPOINT_RNAV:
                    return waypointRNAVTemplate;
                default:
                    return null;
            }
        }

        private Transform[] InstantiateMarkers(string[] identities, uint[] types, bool[] hideFlags, bool isNavaid)
        {
            var markers = new Transform[identities.Length];
            for (var i = 0; i < identities.Length; i++)
            {
                if (hideFlags != null && hideFlags[i]) continue;
                var template = isNavaid ? GetNavaidTemplate(types[i]) : GetWaypointTemplate(types[i]);
                if (template == null) continue;

                markers[i] = InstantiateMarker(template, identities[i]);
            }
            return markers;
        }

        private Transform InstantiateMarker(GameObject template, string identity)
        {
            var marker = VRCInstantiate(template).transform;
            marker.gameObject.name = marker.gameObject.name.Replace("(Clone)", $" {identity}");
            marker.SetParent(transform, false);
            marker.GetComponentInChildren<TextMeshProUGUI>().text = identity;
            return marker;
        }

        private void InitializeMarkerPositions(Transform[] markers, Transform[] sourceTransforms, float scale)
        {
            for (var i = 0; i < markers.Length; i++)
            {
                var marker = markers[i];
                if (marker == null) continue;

                var position = sourceTransforms[i].position * scale;
                marker.localPosition = Vector3.right * position.x + Vector3.up * position.z;
            }
        }

        private void UpdateTransform(float scale)
        {

            var rotation = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.up) + magneticDeclination, Vector3.forward);
            transform.localRotation = rotation;

            var inverseRotation = Quaternion.Inverse(rotation);

            var position = -transform.position * scale;
            transform.localPosition = rotation * (Vector3.right * position.x + Vector3.up * position.z);

            UpdateMarkerRotations(navaidMarkers, inverseRotation);
            UpdateMarkerRotations(waypointMarkers, inverseRotation);
        }

        private void UpdateMarkerRotations(Transform[] markers, Quaternion rotation)
        {
            for (var i = 0; i < markers.Length; i++)
            {
                var marker = markers[i];
                if (marker == null) continue;
                marker.localRotation = rotation;
            }
        }
    }
}
