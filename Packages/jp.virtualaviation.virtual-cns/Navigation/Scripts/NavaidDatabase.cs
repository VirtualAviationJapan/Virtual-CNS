using UdonSharp;
using UnityEngine;
using TMPro;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;
#endif

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NavaidDatabase : UdonSharpBehaviour
    {
        public static NavaidDatabase GetInstance()
        {
            var o = GameObject.Find("NavaidDatabase");
            return o ? o.GetComponent<NavaidDatabase>() : null;
        }

        public static float GetMagneticDeclination()
        {
            var db = GetInstance();
            return db ? db.magneticDeclination : 0;
        }

        public const uint NAVAID_NDB = 1;
        public const uint NAVAID_VOR = 2;
        public const uint NAVAID_DME = 4;
        public const uint NAVAID_ILS = 8;

        public const uint WAYPOINT_WAYPOINT = 0;
        public const uint WAYPOINT_AERDROME = 1;
        public const uint WAYPOINT_RNAV = 2;

        public float magneticDeclination = 0.0f;
        public float frequencyStep = 0.05f;
        public TextMeshProUGUI debugText;

        [HideInInspector] public Transform[] transforms = { };
        [HideInInspector] public uint[] capabilities = { };
        [HideInInspector] public string[] identities = { };
        [HideInInspector] public float[] frequencies = { };
        [HideInInspector] public Transform[] dmeTransforms = { };
        [HideInInspector] public Transform[] glideSlopeTransforms = { };
        [HideInInspector] public bool[] hideFromMaps = { };

        [HideInInspector] public Transform[] waypointTransforms = { };
        [HideInInspector] public string[] waypointIdentities = { };
        [HideInInspector] public uint[] waypointTypes = { };

        public int Count => transforms.Length;
        public bool _HasCapability(int navaidIndex, uint capability) => (capabilities[navaidIndex] & capability) != 0;
        public bool _IsNDB(int navaidIndex) => _HasCapability(navaidIndex, NAVAID_NDB);
        public bool _IsVOR(int navaidIndex) => _HasCapability(navaidIndex, NAVAID_VOR);
        public bool _IsILS(int navaidIndex) => _HasCapability(navaidIndex, NAVAID_ILS);
        public bool _HasDME(int navaidIndex) => _HasCapability(navaidIndex, NAVAID_DME);

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void Awake()
        {
            Setup();
        }
#endif

        private void Start()
        {
                Debug.Log($"[Virtual-CNS][{this}:{GetHashCode():X8}] Initialized with {Count} navaids", gameObject);

            if (debugText)
            {
                var text = "NavaidDatbase";
                for (var i = 0; i < Count; i++)
                {
                    text += $"\n{transforms[i]}\t{identities[i]}\t{frequencies[i]:0.00}";
                }
                debugText.text = text;
            }
        }

        private void Reset()
        {
            gameObject.name = nameof(NavaidDatabase);
        }

        public int _FindIndexByIdentity(string identity)
        {
            for (var i = 0; i < Count; i++)
            {
                if (identities[i] == identity) return i;
            }
            return -1;
        }

        public int _FindIndexByFrequency(float frequency)
        {
            for (var i = 0; i < Count; i++)
            {
                if (Mathf.Abs(frequency - frequencies[i]) < frequencyStep / 2.0f) return i;
            }
            return -1;
        }

        public static float ChannelToFrequency(int channel, bool y)
        {
            return (channel - (channel < 60 ? 17 : 16)) * 0.1f + 108.0f + (y ? 0.05f : 0);
        }

        public static int FrequencyToChannel(float frequency)
        {
            return Mathf.FloorToInt((frequency - 108.0f) * 10.0f + (frequency < 112.3f ? 17 : 27));
        }

        public static bool IsY(float frequency)
        {
            return Mathf.Approximately(frequency % 0.1f, 0.05f);
        }

        public int _FindIndexByChannel(int channel, bool y)
        {
            return _FindIndexByFrequency(ChannelToFrequency(channel, y));
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, Quaternion.AngleAxis(-magneticDeclination, Vector3.up) * Vector3.forward * 10 + transform.position);
        }

        public void Setup()
        {
            var rootObjects = gameObject.scene.GetRootGameObjects();

            var navaids = rootObjects.SelectMany(o => o.GetComponentsInChildren<Navaid>(true)).ToArray();
            transforms = navaids.Select(n => n.transform).ToArray();
            capabilities = navaids.Select(n => (uint)n.capability).ToArray();
            identities = navaids.Select(n => n.identity).ToArray();
            frequencies = navaids.Select(n => n.frequency).ToArray();
            dmeTransforms = navaids.Select(n => n.DmeTransform).ToArray();
            glideSlopeTransforms = navaids.Select(n => n.glideSlope).ToArray();
            hideFromMaps = navaids.Select(n => n.hideFromMap).ToArray();

            var waypoints = rootObjects.SelectMany(o => o.GetComponentsInChildren<Waypoint>(true)).ToArray();
            waypointTransforms = waypoints.Select(w => w.transform).ToArray();
            waypointIdentities = waypoints.Select(w => w.identity).ToArray();
            waypointTypes = waypoints.Select(w => (uint)w.type).ToArray();

            Debug.Log($"[NavaidDatabase] {navaids.Length} navaids and {waypoints.Length} waypoints found.");

            EditorUtility.SetDirty(this);
        }
#endif
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR

    [CustomEditor(typeof(NavaidDatabase))]
    public class NavaidDatabaseEditor : Editor
    {
        public int tabIndex;
        public int navaidIndex;
        public int waypointIndex;
        private NavaidEditor.FrequencyMode frequencyMode;
        private readonly string[] tabs = {
            "Navaids",
            "Waypoints",
        };

        private void NavaidGUI()
        {
            var rootObjects = (target as NavaidDatabase).gameObject.scene.GetRootGameObjects();
            var transforms = rootObjects.SelectMany(o => o.GetComponentsInChildren<Navaid>());
            foreach (var navaid in transforms)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.ObjectField(navaid, typeof(Navaid), true);
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        var serializedNavaid = new SerializedObject(navaid);
                        var property = serializedNavaid.GetIterator();
                        property.NextVisible(true);
                        while (property.NextVisible(false))
                        {
                            switch (property.propertyPath)
                            {
                                case nameof(Navaid.frequency):
                                    frequencyMode = NavaidEditor.FrequencyField(property, frequencyMode, true);
                                    break;
                                default:
                                    EditorGUILayout.PropertyField(property, GUIContent.none);
                                    break;
                            }
                        }
                        if (change.changed) serializedNavaid.ApplyModifiedProperties();
                    }
                }
            }

        }

        private void WaypointGUI()
        {
            var db = target as NavaidDatabase;

            using (new EditorGUILayout.HorizontalScope())
            {
                waypointIndex = EditorGUILayout.Popup(waypointIndex, db.waypointIdentities.ToArray());
            }

            var waypoint = db.waypointTransforms[waypointIndex].GetComponent<Waypoint>();

            EditorGUILayout.ObjectField("Waypoint", waypoint, waypoint.GetType(), true);

            var serializedNavaid = new SerializedObject(waypoint);
            var property = serializedNavaid.GetIterator();
            property.NextVisible(true);
            while (property.NextVisible(false))
            {
                var disabled = false;
                using (new EditorGUI.DisabledGroupScope(disabled))
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }
            serializedNavaid.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            tabIndex = GUILayout.Toolbar(tabIndex, tabs);
            switch (tabIndex)
            {
                case 0:
                    NavaidGUI();
                    break;
                case 1:
                    WaypointGUI();
                    break;
            }

        }

        private static void SetupAll(Scene scene)
        {
            foreach (var db in scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<NavaidDatabase>(true)))
            {
                db.Setup();
            }
        }

        public class BuildCallback : IVRCSDKBuildRequestedCallback
        {
            public int callbackOrder => 11;

            public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
            {
                SetupAll(SceneManager.GetActiveScene());
                return true;
            }
        }
    }
#endif
}
