using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MagneticCompass : FlightDataBusClient
    {
        private void Update()
        {
            _Write(FlightDataFloatValueId.MagneticHeading, (transform.rotation.eulerAngles.y + _Read(FlightDataFloatValueId.MagneticDeclination) + 360) % 360);
        }
    }
}
