using System.Runtime.CompilerServices;
using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
{
    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VORReceiver : FlightDataBusClient
    {
        public int id;
        private Transform vorTransform;

        protected override void OnStart()
        {
        }
    }
}
