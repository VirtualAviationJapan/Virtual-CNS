using UnityEngine;

namespace VirtualCNS
{
    public static class NavigationMath
    {

        public const float VORMaxRange = 200 * 1852.0f ; // m
        public const float VORConeThreshold =  0.6494f; // sin(45deg)
        public const float VORMaxDeviation = 10.0f; // deg

        public const float LocalizerFullDeviation = 0.02443f; // sin(1.4deg)
        public const float LocalizerNarrowDeviation =  0.1736f; // sin(10deg)
        public const float LocalizerWideDeviation = 0.5736f; // sin(35deg)
        public const float LocalizerNarrowMaxRange = 18 * 1852.0f; // m
        public const float LocalizerMaxWideRange = 10 * 1852.0f; // m
        public const float LocalizerMaxBackRange = 10 * 1852.0f; // m
        public const float GlideslopeFullDeviation = 0.01222f; // sin(0.7deg)
        public const float GlideslopeMaxRange = 10 * 1852.0f; // m
        public const float GlideslopeMaxLaterialDeviation = 0.1392f; // sin(8deg)
        public const float GlideslopeMinDeviation = -0.02879f; // sin(-1.65deg) / -55%
        public const float GlideslopeMaxDeviation = 0.03926f; // sin(2.25deg) / +75%

        public static float Remap01(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        public static float Remap11to01(float value)
        {
            return (value + 1.0f) * 0.5f;
        }

        public static float Lerp3(float v0, float v1, float v2, float t0, float t1, float t2, float t)
        {
            return t < t1 ? Mathf.Lerp(v0, v1, Remap01(t, t0, t1)) : Mathf.Lerp(v1, v2, Remap01(t, t1, t2));
        }

        public static float Clamp11(float value)
        {
            return Mathf.Clamp(value, -1, 1);
        }

        public static bool IsBetween(float value, float min, float max)
        {
            return value >= min && value <= max;
        }


        public static float GetBearing(Vector3 relativePosition)
        {
            return Vector3.SignedAngle(Vector3.forward, relativePosition, Vector3.up) + 180;
        }

        public static Vector3 CourseToVector(float course, float magneticDeclination)
        {
            return Quaternion.AngleAxis(course - magneticDeclination, Vector3.up) * Vector3.forward;
        }

        public static bool IsVORAvailable(Vector3 direction, float distance)
        {
            return direction.y < VORConeThreshold && distance < VORMaxRange;
        }

        public static bool IsBehind(float bearing, float course)
        {
            return Mathf.Abs(Mathf.DeltaAngle(bearing, course)) > 90.0f;
        }

        public static float GetVORDeviation(float bearing, float course, bool isBehind)
        {
            return Clamp11(Mathf.DeltaAngle(bearing, isBehind ? course + 180 : course) / VORMaxDeviation);
        }

        public static float GetVORToFrom(Vector3 courseVector, Vector3 relativePosition)
        {
            if (Vector3.Angle(Vector3.up, relativePosition) < 45) return 0.0f;
            return Mathf.Sign(Vector3.Dot(courseVector, relativePosition));
        }

        public static bool InLocalizerRange(Vector3 navaidForward, Vector3 relativePosition)
        {
            return IsBetween(Vector3.Dot(navaidForward, relativePosition) / 1852.0f, -10.0f, 18.0f);
        }

        public static bool IsBehind(Vector3 navaidForward, Vector3 relativePosition)
        {
            return Vector3.Dot(navaidForward, relativePosition) > 0;
        }

        public static float GetLocalizerMaxDeviation(float distance)
        {
            return Lerp3(35.0f, 10.0f, -10.0f, 10.0f, 18.0f, 24.0f, distance / 1852.0f);
        }

        public static float GetCourseDeviation(Vector3 relativePosition, Vector3 courseVector)
        {
            var courseCross = Vector3.Cross(Vector3.up, courseVector).normalized;
            var projected = Vector3.ProjectOnPlane(relativePosition, Vector3.up).normalized;
            return Mathf.Rad2Deg * Mathf.Atan(Vector3.Dot(projected, courseCross));
        }

        public static float GetLocalizerRawDeviation(Vector3 navaidRight, Vector3 direction)
        {
            return Vector3.Dot(navaidRight, direction);
        }

        public static bool IsLocalizerAvailable(float deviation, float distance, bool isBehind)
        {
            var absDeviation = Mathf.Abs(deviation);
            return isBehind && distance <= LocalizerMaxBackRange && absDeviation < LocalizerNarrowDeviation
                    || distance <= LocalizerMaxWideRange && absDeviation < LocalizerWideDeviation
                    || distance <= LocalizerNarrowMaxRange && absDeviation < LocalizerNarrowDeviation;
        }

        public static float ClampLocalizerDeviation(float rawDeviation)
        {
            return Clamp11(rawDeviation / LocalizerFullDeviation);
        }

        public static float GetGlideslopeDeviation(Vector3 navaidForward, Vector3 navaidRight, Vector3 relativePosition)
        {
            return Vector3.SignedAngle(-navaidForward, Vector3.ProjectOnPlane(relativePosition, navaidRight), navaidRight);
        }

        public static float GetGlideslopeRawDeviation(Vector3 glideslopeUp, Vector3 direction)
        {
            return Vector3.Dot(direction, glideslopeUp);
        }

        public static bool IsGlideslopeAvailable(Vector3 glideslopeForward, Vector3 glideslopeRight, Vector3 direction, float distance, float rawDeviation)
        {
            return rawDeviation >= GlideslopeMinDeviation
                && rawDeviation <= GlideslopeMaxDeviation
                && distance <= GlideslopeMaxRange
                && Vector3.Dot(glideslopeForward, direction) < 0
                && Mathf.Abs(Vector3.Dot(direction, glideslopeRight)) < GlideslopeMaxLaterialDeviation;
        }

        public static float ClampGlideslopeDeviation(float rawDeviation)
        {
            return Clamp11(rawDeviation / GlideslopeFullDeviation);
        }
    }
}
