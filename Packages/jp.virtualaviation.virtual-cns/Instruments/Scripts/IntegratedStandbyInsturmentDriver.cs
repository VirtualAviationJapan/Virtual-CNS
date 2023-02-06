
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class IntegratedStandbyInsturmentDriver : UdonSharpBehaviour
    {
        [Header("Attitude Director Indicato")]
        public Transform pitch;
        public Transform roll;
        public float pitchMax = 35.0f, pitchScale = 12.8f;

        [Header("Heading Indicator")]
        public Transform heading;
        public float magneticDeclination = 0.0f;

        [Header("Airspeed Indicator")]
        public Transform asiTape;
        public TextMeshProUGUI asiText;
        public float asiMin = 30.0f, asiMax = 500.0f, asiTapeScale = 6.4f;

        [Header("Altitude Indicator")]
        public Transform altTape;
        public TextMeshProUGUI altText;
        public float altMin = -500.0f, altMax = 64000.0f, altTapeScale = 0.64f;
        public float seaLevel = 0.0f;

        [Header("Misc")]
        public int updateInterval = 3;
        public float velocitySmoothing = 1.0f;

        private Transform origin;
        private Vector3 prevPosition;
        private float ias;
        private int updateIntervalOffset;
        private UdonSharpBehaviour vehicle;

        private void Start()
        {
#if UNITY_ANDROID
            updateInterval *= 2;
#endif
        }

        private void OnEnable()
        {
            if (!origin)
            {
                var rigidbody = GetComponentInParent<Rigidbody>();
                origin = rigidbody ? rigidbody.transform : transform;
            }
            prevPosition = origin.position;
            ias = 0.0f;
            updateIntervalOffset = Random.Range(0, updateInterval);

            if (!vehicle)
            {
                foreach (var udon in origin.GetComponentsInChildren(typeof(UdonBehaviour)))
                {
                    var udonSharp = (UdonSharpBehaviour)udon;
                    if (udonSharp.GetUdonTypeName() == "SaccFlightAndVehicles.SaccAirVehicle")
                    {
                        vehicle = udonSharp;
                        seaLevel = (float)vehicle.GetProgramVariable("SeaLevel");
                    }
                }
            }

            var navaidDatabaseObj = GameObject.Find("NavaidDatabase");
            if (navaidDatabaseObj) magneticDeclination = (float)((UdonBehaviour)navaidDatabaseObj.GetComponent(typeof(UdonBehaviour))).GetProgramVariable("magneticDeclination");
        }

        private void Update()
        {
            if ((Time.frameCount + updateIntervalOffset) % updateInterval != 0) return;

            var position = origin.position;
            var rotation = origin.rotation.eulerAngles;
            var forward = origin.forward;
            var alt = (position.y - seaLevel) * 3.28084f;

            if (vehicle)
            {
                var WindGustiness = (float)vehicle.GetProgramVariable("WindGustiness");
                var WindTurbulanceScale = (float)vehicle.GetProgramVariable("WindTurbulanceScale");
                var WindGustStrength = (float)vehicle.GetProgramVariable("WindGustStrength");
                var Wind = (Vector3)vehicle.GetProgramVariable("Wind");
                var Atmosphere = (float)vehicle.GetProgramVariable("Atmosphere");
                var CurrentVel = (Vector3)vehicle.GetProgramVariable("CurrentVel");

                var TimeGustiness = Time.time * WindGustiness;
                var gustx = TimeGustiness + (origin.position.x * WindTurbulanceScale);
                var gustz = TimeGustiness + (origin.position.z * WindTurbulanceScale);
                var FinalWind = Vector3.Normalize(new Vector3(Mathf.PerlinNoise(gustx + 9000, gustz) - .5f, 0, Mathf.PerlinNoise(gustx, gustz + 9999) - .5f)) * WindGustStrength;
                FinalWind = (FinalWind + Wind) * Atmosphere;

                var airVel = CurrentVel - FinalWind;
                ias = Mathf.Max(Vector3.Dot(airVel, forward), 0.0f) * 1.94384f;
            }
            else
            {
                ias = Mathf.Lerp(Vector3.Dot(position - prevPosition, forward) * 1.94384f, ias, velocitySmoothing * Time.deltaTime);
            }

            prevPosition = position;

            UpdateIndicator(pitch, null, Mathf.Atan(forward.y) * Mathf.Rad2Deg, -pitchMax, pitchMax, pitchScale);

            if (roll) roll.localRotation = Quaternion.AngleAxis(rotation.z, Vector3.back);

            if (heading) heading.localRotation = Quaternion.AngleAxis(rotation.y + magneticDeclination, Vector3.forward);

            UpdateIndicator(asiTape, asiText, ias, asiMin, asiMax, asiTapeScale);
            UpdateIndicator(altTape, altText, alt, altMin, altMax, altTapeScale);
        }

        private void UpdateIndicator(Transform tape, TextMeshProUGUI text, float value, float min, float max, float tapeScale)
        {

            if (tape) tape.localPosition = Mathf.Clamp(value, min, max) * tapeScale * Vector3.down;
            if (text)
            {
                text.text = (value < min || value > max) ? "---" : $"{Mathf.FloorToInt(value)}";
            }
        }
    }
}
