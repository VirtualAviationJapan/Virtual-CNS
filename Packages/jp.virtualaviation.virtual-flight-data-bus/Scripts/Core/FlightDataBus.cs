using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(-1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlightDataBus : AbstractFlightDataBus
    {
        public int maxSubscriberCount = 64;

        private void Start()
        {
            subscribers = new UdonSharpBehaviour[maxSubscriberCount];
            boolSubscriptionMaskList = new uint[maxSubscriberCount];
            floatSubscriptionMaskList = new uint[maxSubscriberCount];
            vector3SubscriptionMaskList = new uint[maxSubscriberCount];
            stringSubscriptionMaskList = new uint[maxSubscriberCount];
        }
    }
}
