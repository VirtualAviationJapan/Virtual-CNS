namespace VirtualFlightDataBus
{
    /// <summary>
    /// Provides utility methods for working with flight data values.
    /// </summary>
    public class FlightDataUtilities
    {
        /// <summary>
        /// Converts knots to meters per second.
        /// </summary>
        public const float Knots = 1.944f;
        /// <summary>
        /// Converts nautical miles to meters.
        /// </summary>
        public const float NM = 1852;

        /// <summary>
        /// Returns a mask with the specified bit set to 1.
        /// </summary>
        /// <param name="n">The bit index to set.</param>
        /// <returns>A mask with the specified bit set to 1.</returns>
        public static ulong GetMask(int n)
        {
            return 1Lu << n;
        }

        /// <summary>
        /// Returns the <see cref="FlightDataBoolValueId"/> with the specified offset from the given base ID.
        /// </summary>
        /// <param name="baseId">The base ID.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The <see cref="FlightDataBoolValueId"/> with the specified offset from the given base ID.</returns>
        public static FlightDataBoolValueId OffsetValueId(FlightDataBoolValueId baseId, int offset)
        {
            return (FlightDataBoolValueId)((int)baseId + offset);
        }


        /// <summary>
        /// Returns the <see cref="FlightDataFloatValueId"/> with the specified offset from the given base ID.
        /// </summary>
        /// <param name="baseId">The base ID.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The <see cref="FlightDataFloatValueId"/> with the specified offset from the given base ID.</returns>
        public static FlightDataFloatValueId OffsetValueId(FlightDataFloatValueId baseId, int offset)
        {
            return (FlightDataFloatValueId)((int)baseId + offset);
        }


        /// <summary>
        /// Returns the <see cref="FlightDataVector3ValueId"/> with the specified offset from the given base ID.
        /// </summary>
        /// <param name="baseId">The base ID.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The <see cref="FlightDataVector3ValueId"/> with the specified offset from the given base ID.</returns>
        public static FlightDataVector3ValueId OffsetValueId(FlightDataVector3ValueId baseId, int offset)
        {
            return (FlightDataVector3ValueId)((int)baseId + offset * 3);
        }

        /// <summary>
        /// Returns the <see cref="FlightDataStringValueId"/> with the specified offset from the given base ID.
        /// </summary>
        /// <param name="baseId">The base ID.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The <see cref="FlightDataStringValueId"/> with the specified offset from the given base ID.</returns>
        public static FlightDataStringValueId OffsetValueId(FlightDataStringValueId baseId, int offset)
        {
            return (FlightDataStringValueId)((int)baseId + offset);
        }
    }

    /// <summary>
    /// IDs for boolean flight data values.
    /// </summary>
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

    /// <summary>
    /// IDs for float flight data values.
    /// </summary>
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

    /// <summary>
    /// IDs for vector3 flight data values.
    /// </summary>
    public enum FlightDataVector3ValueId
    {
        Wind = FlightDataFloatValueId.WindX,
    }

    /// <summary>
    /// IDs for string flight data values.
    /// </summary>
    public enum FlightDataStringValueId
    {
        Nav1Identity,
        Nav2Identity,
        __MAX__,
    }

    /// <summary>
    /// Enumerates IDs for communication radios.
    /// </summary>
    public enum FlightDataComId
    {
        Com1,
        Com2,
        Com3,
    }

    /// <summary>
    /// Enumerates IDs for navigation radios.
    /// </summary>
    public enum FlightDataNavId
    {
        Nav1,
        Nav2,
    }
}
