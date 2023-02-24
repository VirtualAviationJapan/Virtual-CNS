using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MagneticCompass : AbstractFlightDataBusClient
    {
        private void Update()
        {
            _Write(FlightDataFloatValueId.MagneticHeading, (transform.rotation.eulerAngles.y + _Read(FlightDataFloatValueId.MagneticDeclination) + 360) % 360);
        }
    }
}
