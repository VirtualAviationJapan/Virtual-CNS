using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BusDebugger : FlightDataBusClient
    {
        public bool[] debugBools;
        public float[] debugFloats;
        public Vector3[] debugVector3s;
        protected override void OnStart()
        {
            debugBools = Bus.bools;
            debugFloats = Bus.floats;
            debugVector3s = Bus.vector3s;
        }
    }
}
