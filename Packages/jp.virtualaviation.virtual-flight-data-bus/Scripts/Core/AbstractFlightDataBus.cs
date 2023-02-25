using System;
using UdonSharp;

namespace VirtualFlightDataBus
{
    /// <summary>
    /// An abstract base class for a flight data bus that provides access to boolean, float, and string values.
    /// </summary>
    abstract public class AbstractFlightDataBus : UdonSharpBehaviour
    {
        /// <summary>
        /// An array of boolean values that can be accessed by their associated IDs from the <see cref="FlightDataBoolValueId"/> enum.
        /// </summary>
        [NonSerialized] public bool[] bools;

        /// <summary>
        /// An array of float values that can be accessed by their associated IDs from the <see cref="FlightDataFloatValueId"/> enum.
        /// </summary>
        [NonSerialized] public float[] floats;

        /// <summary>
        /// An array of string values that can be accessed by their associated IDs from the <see cref="FlightDataStringValueId"/> enum.
        /// </summary>
        [NonSerialized] public string[] strings;

        /// <summary>
        /// An array of UdonSharpBehaviours that subscribe to updates from this flight data bus.
        /// </summary>
        [NonSerialized] public UdonSharpBehaviour[] subscribers;

        /// <summary>
        /// An array of bit masks indicating which boolean values each subscriber is interested in.
        /// </summary>
        [NonSerialized] public ulong[] boolSubscriptionMaskList;

        /// <summary>
        /// An array of bit masks indicating which float values each subscriber is interested in.
        /// </summary>
        [NonSerialized] public ulong[] floatSubscriptionMaskList;

        /// <summary>
        /// An array of bit masks indicating which Vector3 values each subscriber is interested in.
        /// </summary>
        [NonSerialized] public ulong[] vector3SubscriptionMaskList;

        /// <summary>
        /// An array of bit masks indicating which string values each subscriber is interested in.
        /// </summary>
        [NonSerialized] public ulong[] stringSubscriptionMaskList;
    }
}
