using System;
using UdonSharp;

namespace VirtualFlightDataBus
{
    abstract public class AbstractFlightDataBus : UdonSharpBehaviour
    {
        [NonSerialized] public bool[] bools = new bool[(int)FlightDataBoolValueId.__MAX__];
        [NonSerialized] public float[] floats = new float[(int)FlightDataFloatValueId.__MAX__];
        [NonSerialized] public string[] strings = new string[(int)FlightDataFloatValueId.__MAX__];

        [NonSerialized] public UdonSharpBehaviour[] subscribers;
        [NonSerialized] public ulong[] boolSubscriptionMaskList;
        [NonSerialized] public ulong[] floatSubscriptionMaskList;
        [NonSerialized] public ulong[] vector3SubscriptionMaskList;
        [NonSerialized] public ulong[] stringSubscriptionMaskList;
    }
}
