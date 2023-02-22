using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class StaticDataLoader : FlightDataBusClient
    {
        protected override void OnStart()
        {
            _WriteFloatValue(FlightDataFloatValueId.MagneticDeclination, NavaidDatabase.GetMagneticDeclination());
        }
    }
}
