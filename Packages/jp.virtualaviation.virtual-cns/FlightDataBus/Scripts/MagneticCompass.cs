using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MagneticCompass : FlightDataBusClient
    {
        private void Update()
        {
            _WriteFloatValue(FlightDataFloatValueId.MagneticHeading, (transform.rotation.eulerAngles.y + _ReadFloatValue(FlightDataFloatValueId.MagneticDeclination) + 360) % 360);
        }
    }
}
