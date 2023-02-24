using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(200)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BusDebugger : AbstractFlightDataBusClient
    {
    }
}
