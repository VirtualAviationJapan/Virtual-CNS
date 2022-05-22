using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WindIndicator : UdonSharpBehaviour
    {
        public Transform windDirectionIndicator;
        public TextMeshProUGUI windText, gsText, tasText;
        public int updateInterval = 10;
        private int updateIntervalOffset;
        private UdonSharpBehaviour airVehicle;
        private Transform origin;
        private Rigidbody vehicleRigidbody;
        private float magneticDeclination;
        private void OnEnable()
        {
            updateIntervalOffset = Random.Range(0, updateInterval);
        }

        private void Start()
        {
            vehicleRigidbody = GetComponentInParent<Rigidbody>();

            origin = vehicleRigidbody.transform;
            var entity = (UdonBehaviour)vehicleRigidbody.GetComponent(typeof(UdonBehaviour));
            foreach (var ext in (UdonSharpBehaviour[])entity.GetProgramVariable("ExtensionUdonBehaviours"))
            {
                if (ext.GetUdonTypeName() == "SaccAirVehicle")
                {
                    airVehicle = ext;
                    break;
                }
            }

            var navaidDatabaseObj = GameObject.Find("NavaidDatabase");
            if (navaidDatabaseObj) magneticDeclination = (float)((UdonBehaviour)navaidDatabaseObj.GetComponent(typeof(UdonBehaviour))).GetProgramVariable("magneticDeclination");
        }

        private void Update()
        {
            if ((Time.frameCount + updateIntervalOffset) % updateInterval != 0) return;

            var wind = (Vector3)airVehicle.GetProgramVariable("Wind");
            var windSpeed = wind.magnitude;
            var xzWindDirection = Vector3.ProjectOnPlane(wind, Vector3.up).normalized;
            var windAbsoluteAngle = (Vector3.SignedAngle(Vector3.forward, xzWindDirection, Vector3.up) + magneticDeclination + 540) % 360;
            var windRelativeAngle = Vector3.SignedAngle(origin.forward, xzWindDirection, Vector3.up);

            if (windDirectionIndicator)
            {
                windDirectionIndicator.transform.localRotation = Quaternion.AngleAxis(-windRelativeAngle, Vector3.forward);
            }

            if (windText)
            {
                windText.text = $"{windAbsoluteAngle:000}/{windSpeed * 1.94384f:##0}";
            }

            var airVel = (Vector3)airVehicle.GetProgramVariable("AirVel");
            if (gsText)
            {
                gsText.text = $"<size=75%>GS</size>{vehicleRigidbody.velocity.magnitude * 1.94384f:##0}";
            }

            if (tasText)
            {
                var seaLevel = (float)airVehicle.GetProgramVariable("SeaLevel");
                var tas = (origin.position.y - seaLevel) / 100 / 2 + Mathf.Abs(Vector3.Dot(origin.forward, airVel)) * 1.94384f;
                tasText.text = $"<size=75%>TAS</size>{tas:##0}";
            }
        }
    }
}
