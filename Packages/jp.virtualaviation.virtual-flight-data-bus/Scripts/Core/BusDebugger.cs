using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(200)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BusDebugger : AbstractFlightDataBusClient
    {
        public bool[] debugBools;
        public float[] debugFloats;
        public string[] debugStrings;
        protected override void OnStart()
        {
            debugBools = Bus.bools;
            debugFloats = Bus.floats;
            debugStrings = Bus.strings;
        }
    }
}
