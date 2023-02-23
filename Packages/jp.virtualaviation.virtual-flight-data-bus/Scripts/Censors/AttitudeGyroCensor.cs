using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AttitudeGyroCensor : FlightDataBusClient
    {
        private void Update()
        {
            var eularAngles = transform.rotation.eulerAngles;
            _Write(FlightDataFloatValueId.Pitch, Mathf.Atan(transform.forward.y) * Mathf.Rad2Deg);
            _Write(FlightDataFloatValueId.Roll, eularAngles.z);
            _Write(FlightDataFloatValueId.Heading, (eularAngles.y + _Read(FlightDataFloatValueId.MagneticDeclination) + 360) % 360);
        }
    }
}
