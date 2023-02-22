using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AttitudeGyroCensor : FlightDataBusClient
    {
        private void Update()
        {
            var eularAngles = transform.rotation.eulerAngles;
            _WriteFloatValue(FlightDataFloatValueId.Pitch, Mathf.Atan(transform.forward.y) * Mathf.Rad2Deg);
            _WriteFloatValue(FlightDataFloatValueId.Roll, eularAngles.z);
            _WriteFloatValue(FlightDataFloatValueId.Heading, (eularAngles.y + _ReadFloatValue(FlightDataFloatValueId.MagneticDeclination) + 360) % 360);
        }
    }
}
