using System;
using UdonSharp;
using UnityEngine;
using VRC.Core;

namespace VirtualFlightDataBus
{
    abstract public class AbstractFlightDataBus : UdonSharpBehaviour
    {
        [NonSerialized] public bool[] bools = new bool[(int)FlightDataBoolValueId.__MAX__];
        [NonSerialized] public float[] floats = new float[(int)FlightDataFloatValueId.__MAX__];
        [NonSerialized] public string[] strings = new string[(int)FlightDataFloatValueId.__MAX__];

        [NonSerialized] public UdonSharpBehaviour[] subscribers;
        [NonSerialized] public uint[] boolSubscriptionMaskList;
        [NonSerialized] public uint[] floatSubscriptionMaskList;
        [NonSerialized] public uint[] vector3SubscriptionMaskList;
        [NonSerialized] public uint[] stringSubscriptionMaskList;
    }
}
