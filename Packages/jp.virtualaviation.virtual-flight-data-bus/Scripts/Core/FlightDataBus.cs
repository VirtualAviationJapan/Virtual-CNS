using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    /// <summary>
    /// A concrete implementation of the <see cref="AbstractFlightDataBus"/> class that initializes arrays for values, subscribers and subscription masks.
    /// </summary>
    /// <remarks>
    /// This class should have <see cref="FlightDataBusClient"/> components as its children, which will subscribe to updates from this flight data bus.
    /// </remarks>
    [DefaultExecutionOrder(-1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlightDataBus : AbstractFlightDataBus
    {
        /// <summary>
        /// The maximum number of subscribers that can subscribe to updates from this flight data bus.
        /// </summary>
        public int maxSubscriberCount = 64;

        private void Start()
        {
            bools = new bool[(int)FlightDataBoolValueId.__MAX__];
            floats = new float[(int)FlightDataFloatValueId.__MAX__];
            strings = new string[(int)FlightDataStringValueId.__MAX__];
            subscribers = new UdonSharpBehaviour[maxSubscriberCount];
            boolSubscriptionMaskList = new ulong[maxSubscriberCount];
            floatSubscriptionMaskList = new ulong[maxSubscriberCount];
            vector3SubscriptionMaskList = new ulong[maxSubscriberCount];
            stringSubscriptionMaskList = new ulong[maxSubscriberCount];
        }
    }
}
