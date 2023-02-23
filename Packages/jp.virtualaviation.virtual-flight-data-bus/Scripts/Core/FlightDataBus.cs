using System;
using UdonSharp;
using UnityEngine;
using VRC.Core;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(-1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlightDataBus : UdonSharpBehaviour
    {
        public const float Knots = 1.944f;
        public const float NM = 1852;

        public int maxSubscriberCount = 64;

        [NonSerialized] public bool[] bools = new bool[(int)FlightDataBoolValueId.__MAX__];
        [NonSerialized] public float[] floats = new float[(int)FlightDataFloatValueId.__MAX__];
        [NonSerialized] public Vector3[] vector3s = new Vector3[(int)FlightDataVector3ValueId.__MAX__];

        [NonSerialized] public UdonSharpBehaviour[] subscribers;
        [NonSerialized] public uint[] boolSubscriptionMaskList;
        [NonSerialized] public uint[] floatSubscriptionMaskList;
        [NonSerialized] public uint[] vector3SubscriptionMaskList;

        private void Start()
        {
            subscribers = new UdonSharpBehaviour[maxSubscriberCount];
            boolSubscriptionMaskList = new uint[maxSubscriberCount];
            floatSubscriptionMaskList = new uint[maxSubscriberCount];
            vector3SubscriptionMaskList = new uint[maxSubscriberCount];
        }

        public static FlightDataBoolValueId OffsetValueId(FlightDataBoolValueId baseId, int offset)
        {
            return (FlightDataBoolValueId)((int)baseId + offset);
        }

        public static FlightDataFloatValueId OffsetValueId(FlightDataFloatValueId baseId, int offset)
        {
            return (FlightDataFloatValueId)((int)baseId + offset);
        }
        public static FlightDataVector3ValueId OffsetValueId(FlightDataVector3ValueId baseId, int offset)
        {
            return (FlightDataVector3ValueId)((int)baseId + offset);
        }
    }

    public enum FlightDataType
    {
        Bool,
        Float,
        Vector3,
    }

    public enum FlightDataBoolValueId
    {
        Com1Mic,
        Com2Mic,
        Com3Mic,
        Nav1Tuned,
        Nav2Tuned,
        Nav1Back,
        Nav2Back,
        Nav1ILS,
        Nav2LocalizerCaptured,
        Nav1GlideslopeCaptured,
        Nav2GlideslopeCaptured,
        __MAX__,
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
        Com1Frequency,
        Com2Frequency,
        Com3Frequency,
        Nav1Frequency,
        Nav2Frequency,
        Nav1Course,
        Nav2Course,
        Nav1Bearing,
        Nav2Bearing,
        Nav1CourseDeviation,
        Nav2CourseDeviation,
        Nav1Distance,
        Nav2Distance,
        Nav1VerticalDeviation,
        Nav2VerticalDeviation,
        __MAX__,
    }

    public enum FlightDataVector3ValueId
    {
        Wind,
        __MAX__,
    }
}
