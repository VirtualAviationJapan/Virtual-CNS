using UdonSharp;
using UnityEngine;
using VirtualCNS;

namespace VirtualFlightDataBus
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NavaidDatabaseLoader : FlightDataBusClient
    {
        protected override void OnStart()
        {
            _Write(FlightDataFloatValueId.MagneticDeclination, NavaidDatabase.GetMagneticDeclination());
        }
    }
}
