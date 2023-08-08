using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Enums;

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TCAS : UdonSharpBehaviour
    {
        private const float NauticalMile  = 1852;
        private const float Feet = 3.28084f;
        private const float FPM = 196.85f;
        private const float dataTimeout = 5;
        private const float minVerticalSpeed = 10; // feet/min;
        private const float alertInterval = 3; // s
        private const float minTrafficSpeed = 1; // m/s
        private const float minDistance = 5; // m
        private const float minTrafficRadioAltitude = 360; // feet

        private const byte STATE_CLEAR = 0;
        private const byte STATE_PROXIMIATE = 1;
        private const byte STATE_TA = 10;
        private const byte STATE_RA = 20;
        private const byte STATE_RA_CLIMB = 21;
        private const byte STATE_RA_DECEND = 22;
        private const byte STATE_RA_INCREASE_CLIMB = 23;
        private const byte STATE_RA_INCREASE_DECEND = 24;

        public GameObject trafficIconTemplate;
        public Transform displayOrigin;
        public GameObject vsClimb;
        public GameObject vsDecend;
        public GameObject vsIncreaseClimb;
        public GameObject vsIncreaseDecend;
        public float maxVerticalSpeed = 6000;
        public float range = 6;
        public float displaySize = 1024;
        public AudioSource audioSource;
        public AudioClip taTraffic, raClimb, raDescend, raIncreaseClimb, raIncreaseDecend, cc;

        public LayerMask trafficLayerMask = 1 << 17;
        public LayerMask groundLayerMask = 1 << 0 | 1 << 4 | 1 << 11;
        public int updateInterval = 33;

        private int vehicleId;
        private Rigidbody vehicleRigidbody;
        private Transform vehicleTransform;
        private float lastAlertedTime;
        private byte prevState;
        private void Start()
        {
            vehicleRigidbody = GetComponentInParent<Rigidbody>();
            vehicleTransform = vehicleRigidbody ? vehicleRigidbody.transform : transform;
            vehicleId = $"{Networking.LocalPlayer.playerId}:{vehicleTransform.gameObject.name}".GetHashCode();

            SetVerticalSpeed(null);

            updateIntervalOffset = Random.Range(0, updateInterval);
        }

        private void OnEnable()
        {
            updateIntervalOffset = Random.Range(0, updateInterval);

        }

        private int gcIndex;
        private void LateUpdate()
        {
            if ((Time.frameCount + updateIntervalOffset) % updateInterval != 0) return;

            var time = Time.time;

            TableGC(gcIndex, time);
            gcIndex = (gcIndex + 1) % TableSize;

            var vehicleHeading = Vector3.SignedAngle(Vector3.forward, vehicleTransform.forward, Vector3.up);
            var vehiclePosition = transform.position;

            RaycastHit hitInfo;
            var hit = Physics.Raycast(vehicleTransform.position, Vector3.down, out hitInfo, 5000 / Feet, groundLayerMask, QueryTriggerInteraction.Collide);
            var radioAltitude = hit ? hitInfo.distance * Feet : float.MaxValue;
            if (radioAltitude < minTrafficRadioAltitude) return;

            var groundAltitude = vehiclePosition.y * Feet - radioAltitude;

            var colliders = Physics.OverlapSphere(vehicleTransform.position, 30 * NauticalMile, trafficLayerMask, QueryTriggerInteraction.Ignore);
            var currentState = STATE_CLEAR;

            vehicleId = $"{Networking.GetOwner(vehicleTransform.gameObject).playerId}:{vehicleTransform.gameObject.name}".GetHashCode();

            foreach (var collider in colliders)
            {
                if (!collider) continue;

                var rigidbody = collider.attachedRigidbody;
                if (!rigidbody || rigidbody.isKinematic) continue;

                var trafficObject = rigidbody.gameObject;
                var key =  $"{Networking.GetOwner(trafficObject).playerId}:{trafficObject.name}".GetHashCode();
                if (key == vehicleId) continue;

                var trafficState = UpdateTraffic(key, time, vehiclePosition, groundAltitude, vehicleHeading, rigidbody.position);
                if (trafficState > currentState) currentState = trafficState;
            }

            AudioClip alertSound = null;
            if (currentState == STATE_TA) alertSound = taTraffic;
            else if (currentState == STATE_RA_CLIMB) alertSound = raClimb;
            else if (currentState == STATE_RA_DECEND) alertSound = raDescend;
            else if (currentState == STATE_RA_INCREASE_CLIMB) alertSound = raIncreaseClimb;
            else if (currentState == STATE_RA_INCREASE_DECEND) alertSound = raIncreaseDecend;
            else if (currentState < STATE_TA && prevState >= STATE_RA) alertSound = cc;

            var stateChanged = currentState != prevState;
            if (alertSound && stateChanged || currentState >= STATE_TA && (time - lastAlertedTime) >= alertInterval)
            {
                audioSource.PlayOneShot(alertSound);
                lastAlertedTime = time;
            }

            if (stateChanged)
            {
                if (currentState == STATE_RA_CLIMB)  SetVerticalSpeed(vsClimb);
                else if (currentState == STATE_RA_DECEND)  SetVerticalSpeed(vsDecend);
                else if (currentState == STATE_RA_INCREASE_CLIMB)  SetVerticalSpeed(vsIncreaseClimb);
                else if (currentState == STATE_RA_INCREASE_DECEND)  SetVerticalSpeed(vsIncreaseDecend);
                else SetVerticalSpeed(null);
            }
            prevState = currentState;
        }

        private readonly float Log2Scaler = 1.0f / Mathf.Log(2, 2);
        private float ApplyLog2Curve(float value)
        {
            return Mathf.Log(Mathf.Abs(value) + 1, 2) * Log2Scaler * Mathf.Sign(value);
        }
        private float GetFillAmount(float verticalSpeed)
        {
            return ApplyLog2Curve(Mathf.Clamp(verticalSpeed / maxVerticalSpeed, -1, 1)) * 0.5f + 0.5f;
        }

        private void SetVerticalSpeed(GameObject o)
        {
            SetVerticalSpeedActive(o, vsClimb);
            SetVerticalSpeedActive(o, vsDecend);
            SetVerticalSpeedActive(o, vsIncreaseClimb);
            SetVerticalSpeedActive(o, vsIncreaseDecend);
        }
        private void SetVerticalSpeedActive(GameObject activeObject, GameObject vsObject)
        {
            if (!vsObject) return;
            var active = activeObject == vsObject;
            if (active != vsObject.activeSelf) vsObject.SetActive(active);
        }

        #region HashTable
        private const int TableSize = 128;
        private int[] trafficKeys = new int[TableSize];
        private Vector3[] trafficPositions = new Vector3[TableSize];
        private float[] trafficUpdatedTimes = new float[TableSize];
        private Transform[] trafficIcons = new Transform[TableSize];
        private Image[] trafficIconImages = new Image[TableSize];
        private Transform[] trafficIconVSArrows = new Transform[TableSize];
        private float[] trafficDistances = new float[TableSize];
        private TextMeshProUGUI[] trafficIconAltitudeTexts = new TextMeshProUGUI[TableSize];
        private byte[] trafficStatus = new byte[TableSize];
        private int updateIntervalOffset;

        private int GetIndex(int key)
        {
            for (var i = Mathf.Abs(key) % TableSize; i < TableSize; i++)
            {
                if (trafficKeys[i] == -1 || trafficKeys[i] == key) return i;
            }
            return -1;
        }

        private byte UpdateTraffic(int key, float time, Vector3 vehiclePosition, float groundAltitude, float vehicleHeading, Vector3 trafficPosition)
        {
            if ((trafficPosition.y - groundAltitude) * Feet < minTrafficRadioAltitude) return STATE_CLEAR;

            var trafficState = STATE_CLEAR;
            var index = GetIndex(key);
            if (index < 0) return trafficState;

            if (trafficKeys[index] == key)
            {
                var prevTrafficPosition = trafficPositions[index];
                var deltaTime = time - trafficUpdatedTimes[index];

                if (deltaTime < Time.fixedDeltaTime) return trafficState;

                var relativePosition = trafficPosition - vehiclePosition;
                var relativeFL = Mathf.CeilToInt(relativePosition.y * Feet / 100);

                var trafficVelocity = trafficPosition - prevTrafficPosition;
                var verticalSpeed = trafficVelocity.y * FPM / deltaTime;
                var distance = relativePosition.magnitude;
                var closureSpeed = -(distance - trafficDistances[index]) / deltaTime;
                var estimatedTime = distance / closureSpeed;

                if (!trafficIcons[index])
                {
                    var iconObject = Instantiate(trafficIconTemplate);
                    iconObject.name = $"Traffic ({index + 1})";

                    var iconTransform = iconObject.transform;
                    trafficIcons[index] = iconTransform;

                    iconTransform.SetParent(displayOrigin, false);

                    trafficIconImages[index] = iconObject.GetComponent<Image>();
                    trafficIconVSArrows[index] = iconTransform.Find("VS");
                    trafficIconAltitudeTexts[index] = iconTransform.Find("ALT").GetComponent<TextMeshProUGUI>();
                }

                var trafficIcon = trafficIcons[index];
                var trafficIconObject = trafficIcon.gameObject;
                if (!trafficIconObject.activeSelf) trafficIconObject.SetActive(true);

                var scaledPosition = Quaternion.AngleAxis(-vehicleHeading, Vector3.up) * relativePosition * displaySize / (2.0f * range * NauticalMile);
                trafficIcon.localPosition = Vector3.right * scaledPosition.x + Vector3.up * scaledPosition.z;

                var notLevelFlight = verticalSpeed >= minVerticalSpeed || verticalSpeed <= -minVerticalSpeed;
                var trafficIconVSArrow = trafficIconVSArrows[index];
                if (trafficIconVSArrow.gameObject.activeSelf != notLevelFlight) trafficIconVSArrow.gameObject.SetActive(notLevelFlight);
                if (notLevelFlight) trafficIconVSArrow.localScale = Vector3.right + Vector3.up * Mathf.Sign(verticalSpeed) + Vector3.forward;

                trafficIconAltitudeTexts[index].text = $"{relativeFL:+00;-00;}";
                var trafficIconAltitudeTextTransform = trafficIconAltitudeTexts[index].transform;
                var trafficIconAltitudeTextPosition = trafficIconAltitudeTextTransform.localPosition;
                trafficIconAltitudeTextTransform.localPosition = Vector3.ProjectOnPlane(trafficIconAltitudeTextPosition, Vector3.up) + Vector3.up * Mathf.Abs(Vector3.Dot(trafficIconAltitudeTextPosition, Vector3.up)) * Mathf.Sign(-relativeFL);

                var verticalDistance = Mathf.Abs(relativeFL * 100);
                if (estimatedTime >= 0 && distance > minDistance && trafficVelocity.magnitude > minTrafficSpeed)
                {
                    if (estimatedTime < 10 && verticalDistance < 600) trafficState = vehicleId > key ? STATE_RA_INCREASE_CLIMB : STATE_RA_INCREASE_DECEND;
                    else if (estimatedTime < 30 && verticalDistance < 600) trafficState = vehicleId > key ? STATE_RA_CLIMB : STATE_RA_DECEND;
                    else if (estimatedTime < 48 && verticalDistance < 850) trafficState = STATE_TA;
                }
                if (trafficState == STATE_CLEAR && (estimatedTime >= 0 && estimatedTime < 48 || verticalDistance < 850)) trafficState = STATE_PROXIMIATE;

                if (trafficState != trafficStatus[index])
                {
                    if (trafficState >= STATE_RA) SetTrafficIconSymbol(trafficIcon, "RA");
                    else if (trafficState >= STATE_TA) SetTrafficIconSymbol(trafficIcon, "TA");
                    else if (trafficState >= STATE_PROXIMIATE) SetTrafficIconSymbol(trafficIcon, "PROXIMIATE");
                    else SetTrafficIconSymbol(trafficIcon, "OTHER");
                }

                trafficDistances[index] = distance;
                trafficStatus[index] = trafficState;
            }

            trafficKeys[index] = key;
            trafficPositions[index] = trafficPosition;
            trafficUpdatedTimes[index] = time;

            return trafficState;
        }

        private readonly string[] TrafficIconSymbolNames = {
            "RA",
            "TA",
            "PROXIMIATE",
            "OTHER",
        };
        private void SetTrafficIconSymbol(Transform trafficIcon, string symbolName)
        {
            foreach (var n in TrafficIconSymbolNames)
            {
                var s = trafficIcon.Find(n);
                if (!s) continue;
                s.gameObject.SetActive(n == symbolName);
            }
        }

        private void TableGC(int index, float time)
        {
            if (trafficKeys[index] == -1) return;
            if (time - trafficUpdatedTimes[index] > dataTimeout)
            {
                trafficKeys[index] = -1;
                var iconTransform = trafficIcons[index];
                if (iconTransform) iconTransform.gameObject.SetActive(false);
            }
        }
        #endregion
    }
}
