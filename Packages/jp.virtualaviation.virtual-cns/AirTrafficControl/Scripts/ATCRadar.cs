using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using UdonToolkit;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using VRC.SDKBase.Editor.BuildPipeline;
#endif

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(-100)] // Before OneShotCamera
    public class ATCRadar : UdonSharpBehaviour
    {
        public Transform seaLevel;
        public Transform raderOrigin;
        public GameObject symbolTemplate;
        public Transform symbolContainer;
        public float range = 10.0f;
        public float uiRadius = 512.0f;
        [Range(-360.0f, 360.0f)] public float headingOffset = 0.0f;
        public Camera terrainCamera;

        [Disabled][ListView("Traffics")] public Transform[] traffics = { };
        [Disabled][ListView("Traffics")] public string[] tailNumbers = { };
        [Disabled][ListView("Traffics")] public string[] callsigns = { };
        [Disabled][ListView("Traffics")] public GameObject[] ownerDetectors = { };

        private Transform[] symbols = { };
        private TextMeshProUGUI[] symbolTexts = { };
        private Vector3[] previousPositions = { };
        private float[] previousTimes = { };

        private void Start()
        {
            if (seaLevel == null) seaLevel = transform;
            if (raderOrigin == null) raderOrigin = transform;
            if (symbolContainer == null) symbolContainer = transform;

            symbols = new Transform[traffics.Length];
            symbolTexts = new TextMeshProUGUI[traffics.Length];
            previousPositions = new Vector3[traffics.Length];
            previousTimes = new float[traffics.Length];

            for (var i = 0; i < traffics.Length; i++)
            {
                var obj = Instantiate(symbolTemplate);
                obj.SetActive(false);
                obj.name = obj.name.Replace("(Clone)", $"_{tailNumbers[i]}");
                var symbol = obj.transform;
                symbols[i] = symbol;
                symbol.SetParent(symbolContainer, false);

                var symbolText = symbol.GetComponentInChildren<TextMeshProUGUI>();
                symbolTexts[i] = symbolText;
                symbolText.text = tailNumbers[i];
            }

            if (terrainCamera) terrainCamera.orthographicSize = range * 1852;
        }

        private void Update()
        {
            var time = Time.time;
            var scale = uiRadius / (range * 1852);

            var index = Time.frameCount % Mathf.Max(traffics.Length, 1);

            var traffic = traffics[index];
            var tailNumber = tailNumbers[index];
            var callsign = callsigns[index];
            var symbol = symbols[index];
            var symbolText = symbolTexts[index];
            var ownerDetector = ownerDetectors[index];

            var position = traffic.position - raderOrigin.position;
            var offsetRotation = Quaternion.AngleAxis(headingOffset, Vector3.forward);

            if (position.magnitude <= range * 1852)
            {
                SetActive(symbol, true);

                var altitude = (position.y - seaLevel.position.y) * 3.28084f;

                var groundVelocity = Vector3.ProjectOnPlane(position - previousPositions[index], Vector3.up);
                var groundSpeed = groundVelocity.magnitude / (time - previousTimes[index]) * 1.94384f;
                previousPositions[index] = position;
                previousTimes[index] = time;

                var rotation = offsetRotation * Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, Vector3.Scale(groundVelocity, new Vector3(-1, 1, 1)), Vector3.up), Vector3.forward);

                symbol.transform.localPosition = offsetRotation * (Vector3.right * position.x + Vector3.up * position.z) * scale;
                symbol.transform.localRotation = rotation;
                symbol.transform.localScale = Vector3.one * (groundSpeed < 5 ? 0.25f : 1.0f);

                var owner = Networking.GetOwner(ownerDetector).displayName;

                symbolText.transform.localRotation = Quaternion.Inverse(rotation);
                symbolText.text = $"{(string.IsNullOrEmpty(callsign) ? tailNumber : callsign)}\n{Mathf.FloorToInt(groundSpeed / 10.0f):00} {Mathf.FloorToInt(altitude / 100.0f):00}\n{owner}";
            }
            else SetActive(symbol, false);
        }

        private void SetActive(Transform symbol, bool value)
        {
            var o = symbol.gameObject;
            if (o.activeSelf != value) o.SetActive(value);
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        [Button("Force Refresh Now", true)]
        public void Setup()
        {
            var rootObjects = gameObject.scene.GetRootGameObjects();
            var trafficSources = rootObjects.SelectMany(o => o.GetComponentsInChildren<TailNumberManager>()).ToArray();

            traffics = trafficSources.Select(s => s.transform).ToArray();
            tailNumbers = trafficSources.Select(s => s.tailNumber).ToArray();
            callsigns = trafficSources.Select(s => s.callsign).ToArray();
            ownerDetectors = trafficSources.Select(s => s.gameObject.GetComponentsInChildren<UdonSharpBehaviour>().FirstOrDefault(u => u.GetType().Name == "EngineController" || u.GetType().Name == "SaccFlightAndVehicles.SaccAirVehicle")?.gameObject ?? s.gameObject).ToArray();
        }

        private static void SetupAll(Scene scene)
        {
            foreach (var rader in scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<ATCRadar>(true)))
            {
                rader.Setup();
                EditorUtility.SetDirty(rader);
            }
        }


        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            EditorApplication.playModeStateChanged += (PlayModeStateChange e) => {
                if (e == PlayModeStateChange.EnteredPlayMode) SetupAll(SceneManager.GetActiveScene());
            };
        }

        public class ESFASceneSetupCallback : Editor, IVRCSDKBuildRequestedCallback
        {
            public int callbackOrder => 11;

            public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
            {
                SetupAll(SceneManager.GetActiveScene());
                return true;
            }
        }
#endif
    }
}
