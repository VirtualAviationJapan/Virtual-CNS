using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(20)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AirDataComputer : AbstractFlightDataBusClient
    {
        private void Update()
        {
            _Write(FlightDataFloatValueId.AngleOfAttack, Mathf.Atan2(_Read(FlightDataFloatValueId.VerticalSpeed), _Read(FlightDataFloatValueId.TAS)) * Mathf.Rad2Deg);
        }
    }
}
