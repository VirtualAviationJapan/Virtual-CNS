using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BusDebugger : FlightDataBusClient
    {
        public float[] debugFloats;
        public Vector3[] debugVector3s;
        protected override void OnStart()
        {
            debugFloats = Bus.floats;
            debugVector3s = Bus.vector3s;
        }
    }
}
