﻿using UdonSharp;
using UnityEngine;
using VRC.Udon;
using System;
using UnityEngine.SceneManagement;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UdonSharpEditor;
using VRC.SDKBase.Editor.BuildPipeline;
#endif

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NavaidDatabase : UdonSharpBehaviour
    {
        public const uint NAVAID_NDB = 1;
        public const uint NAVAID_VOR = 2;
        public const uint NAVAID_DME = 4;
        public const uint NAVAID_ILS = 8;

        public const uint WAYPOINT_WAYPOINT = 0;
        public const uint WAYPOINT_AERDROME = 1;
        public const uint WAYPOINT_RNAV = 2;

        public float magneticDeclination = 0.0f;

        public Transform[] transforms = { };
        public uint[] capabilities = { };
        public string[] identities = { };
        public float[] frequencies = { };
        public Transform[] dmeTransforms = { };
        public Transform[] glideSlopeTransforms = { };
        public bool[] hideFromMaps = { };
        public float frequencyStep = 0.05f;

        public Transform[] waypointTransforms = { };
        public string[] waypointIdentities = { };
        public uint[] waypointTypes = { };

        public int Count => transforms.Length;
        public bool _HasCapability(int navaidIndex, uint capability) => (capabilities[navaidIndex] & capability) != 0;
        public bool _IsNDB(int navaidIndex) => _HasCapability(navaidIndex, NAVAID_NDB);
        public bool _IsVOR(int navaidIndex) => _HasCapability(navaidIndex, NAVAID_VOR);
        public bool _IsILS(int navaidIndex) => _HasCapability(navaidIndex, NAVAID_ILS);
        public bool _HasDME(int navaidIndex) => _HasCapability(navaidIndex, NAVAID_DME);

        private void Start()
        {
            Debug.Log($"[Virtual-CNS][{this}:{GetHashCode():X8}] Initialized", gameObject);
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

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, Quaternion.AngleAxis(-magneticDeclination, Vector3.up) * Vector3.forward * 10 + transform.position);
        }
        public static IEnumerable<T> GetUdonSharpComponentsInScene<T>(Scene scene) where T : UdonSharpBehaviour
        {
            return scene.GetRootGameObjects()
                .SelectMany(o => o.GetComponentsInChildren<UdonBehaviour>(true))
                .Where(UdonSharpEditorUtility.IsUdonSharpBehaviour)
                .Select(UdonSharpEditorUtility.GetProxyBehaviour)
                .Select(u => u as T)
                .Where(u => u != null);
        }

        public void Setup()
        {
            var rootObjects = gameObject.scene.GetRootGameObjects();
            var navaids = rootObjects.SelectMany(o => o.GetComponentsInChildren<Navaid>()).ToArray();
            transforms = navaids.Select(n => n.transform).ToArray();
            capabilities = navaids.Select(n => (uint)n.capability).ToArray();
            identities = navaids.Select(n => n.identity).ToArray();
            frequencies = navaids.Select(n => n.frequency).ToArray();
            dmeTransforms = navaids.Select(n => n.DmeTransform).ToArray();
            glideSlopeTransforms = navaids.Select(n => n.glideSlope).ToArray();
            hideFromMaps = navaids.Select(n => n.hideFromMap).ToArray();

            var waypoints = rootObjects.SelectMany(o => o.GetComponentsInChildren<Waypoint>());
            waypointTransforms = waypoints.Select(w => w.transform).ToArray();
            waypointIdentities = waypoints.Select(w => w.identity).ToArray();
            waypointTypes = waypoints.Select(w => (uint)w.type).ToArray();
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
            if (GUILayout.Button("Force Refresh", EditorStyles.miniButton, GUILayout.ExpandWidth(false))) SetupAll((target as NavaidDatabase).gameObject.scene);

            var transformsProperty = serializedObject.FindProperty(nameof(NavaidDatabase.transforms));
            var navaidCount = transformsProperty.arraySize;
            foreach (var i in Enumerable.Range(0, navaidCount))
            {
                var navaid = (transformsProperty.GetArrayElementAtIndex(i).objectReferenceValue as Transform).GetComponent<Navaid>();
                if (!navaid) continue;

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

                if (GUILayout.Button("Force Refresh", EditorStyles.miniButton, GUILayout.ExpandWidth(false))) SetupAll(db.gameObject.scene);
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
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(NavaidDatabase.magneticDeclination)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(NavaidDatabase.frequencyStep)));
            serializedObject.ApplyModifiedProperties();

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
            foreach (var db in NavaidDatabase.GetUdonSharpComponentsInScene<NavaidDatabase>(scene))
            {
                db.Setup();
                UdonSharpEditorUtiliry.CopyProxyToUdon(db);
            }
        }

        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            EditorApplication.playModeStateChanged += (PlayModeStateChange e) =>
            {
                if (e == PlayModeStateChange.EnteredPlayMode) SetupAll(SceneManager.GetActiveScene());
            };
        }

        public class BuildCallback : IVRCSDKBuildRequestedCallback
        {
            public int callbackOrder => 10;

            public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
            {
                SetupAll(SceneManager.GetActiveScene());
                return true;
            }
        }
    }
#endif
}
