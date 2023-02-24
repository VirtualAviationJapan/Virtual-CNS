using System;
using UdonSharp;
using UnityEngine;
using VRC.Core;

namespace VirtualFlightDataBus
{
    public class FlightDataUtilities
    {
        public const float Knots = 1.944f;
        public const float NM = 1852;
        public static ulong GetMask(int n)
        {
            return 1Lu << n;
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
            return (FlightDataVector3ValueId)((int)baseId + offset * 3);
        }

        public static FlightDataStringValueId OffsetValueId(FlightDataStringValueId baseId, int offset)
        {
            return (FlightDataStringValueId)((int)baseId + offset);
        }
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
        Nav2ILS,
        Nav1GlideslopeCaptured,
        Nav2GlideslopeCaptured,
        __MAX__,
    }

    public enum FlightDataFloatValueId
    {
        WindX,
        WindY,
        WindZ,
        Pitch,
        Roll,
        Heading,
        TAS,
        Altitude,
        VerticalSpeed,
        SeaLevel,
        MagneticHeading,
        MagneticDeclination,
        AngleOfAttack,
        LateralLoad,
        VerticalLoad,
        LongitudinalLoad,
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
        Wind = FlightDataFloatValueId.WindX,
    }

    public enum FlightDataStringValueId
    {
        Nav1Identity,
        Nav2Identity,
        __MAX__,
    }

    public enum FlightDataComId
    {
        Com1,
        Com2,
        Com3,
    }

    public enum FlightDataNavId
    {
        Nav1,
        Nav2,
    }
}
