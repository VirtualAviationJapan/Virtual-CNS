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
            boolSubscriptionMaskList = new ulong[maxSubscriberCount];
            floatSubscriptionMaskList = new ulong[maxSubscriberCount];
            vector3SubscriptionMaskList = new ulong[maxSubscriberCount];
            stringSubscriptionMaskList = new ulong[maxSubscriberCount];
        }
    }
}
