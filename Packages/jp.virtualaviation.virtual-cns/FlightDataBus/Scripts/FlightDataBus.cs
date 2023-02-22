using System;
using UdonSharp;
using UnityEngine;
using VRC.Core;

namespace VirtualAviationJapan.FlightDataBus
{
    [DefaultExecutionOrder(-1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlightDataBus : UdonSharpBehaviour
    {
        public const float Knots = 1.944f;

        public int maxSubscriberCount = 64;

        [NonSerialized] public float[] floats = new float[(int)FlightDataFloatValueId.Max];
        [NonSerialized] public Vector3[] vector3s = new Vector3[(int)FlightDataVector3ValueId.Max];

        [NonSerialized] public UdonSharpBehaviour[] subscribers;
        [NonSerialized] public uint[] floatSubscriptionMaskList;
        [NonSerialized] public uint[] vector3SubscriptionMaskList;

        private void Start()
        {
            subscribers = new UdonSharpBehaviour[maxSubscriberCount];
            floatSubscriptionMaskList = new uint[maxSubscriberCount];
            vector3SubscriptionMaskList = new uint[maxSubscriberCount];
        }

    }

    public enum FlightDataType
    {
        Float,
        Vector3,
    }

    public enum FlightDataFloatValueId
    {
        Pitch,
        Roll,
        Heading,
        TAS,
        Altitude,
        VerticalSpeed,
        SeaLevel,
        MagneticHeading,
        MagneticDeclination,
        Nav1Frequency,
        Nav1Course,
        Nav2Frequency,
        Nav2Course,
        Max,
    }

    public enum FlightDataVector3ValueId
    {
        Wind,
        Max,
    }
}
