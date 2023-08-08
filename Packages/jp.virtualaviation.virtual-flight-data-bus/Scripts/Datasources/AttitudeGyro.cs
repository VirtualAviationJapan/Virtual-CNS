using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(0)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AttitudeGyro : AbstractFlightDataBusClient
    {
        private void Update()
        {
            var forward = transform.forward;
            _Write(FlightDataFloatValueId.Pitch, Mathf.Atan(forward.y) * Mathf.Rad2Deg);
            _Write(FlightDataFloatValueId.Roll, Vector3.SignedAngle(transform.up, Vector3.ProjectOnPlane(Vector3.up, forward).normalized, forward));
            _Write(FlightDataFloatValueId.Heading, (transform.rotation.eulerAngles.y + _Read(FlightDataFloatValueId.MagneticDeclination) + 360) % 360);
        }
    }
}
