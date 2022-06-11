
using UdonSharp;
using UdonToolkit;
using UnityEngine;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace VirtualAviationJapan
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CDIAnimationDriver : UdonSharpBehaviour
    {
        public NavSelector navaidSelector;
        public Animator cdiAnimator;
        [Popup("animatorFloat", "@cdiAnimator")] public string courseFloatParameter;
        [Popup("animatorFloat", "@cdiAnimator")] public string courseDeviationFloatParameter;
        [Popup("animatorFloat", "@cdiAnimator")] public string glideSlopeDeviationFloatParameter;
        [Popup("animatorFloat", "@cdiAnimator")] public string toFromFloatParameeter;
        [Popup("animatorBool", "@cdiAnimator")] public string glideSlopeBoolParameter;
        public int navaidUpdateInterval = 10;
        public float dampTime = 0.1f;
        private float magneticDeclination;
        private int navaidUpdateIntervalOffset;
        private bool tuned;
        private Transform navaidTransform;
        private Vector3 navaidPosition;
        private Vector3 navaidForward;
        private bool isVOR;
        private bool isILS;
        private Transform glideSlopeTransform;
        private Vector3 glideSlopePosition;
        private Vector3 glideSlopeForward;
        private Vector3 glideSlopeUp;
        private Vector3 glideSlopeRight;
        private float glideSlopeAngle;
        private float glideSlopeMinDeviation;
        private float deltaTime;
        private float glideSlopeMaxDeviation;

        private void Start()
        {
            var navaidDatabaseObj = GameObject.Find(nameof(NavaidDatabase));
            if (navaidDatabaseObj) magneticDeclination = navaidDatabaseObj.GetComponent<NavaidDatabase>().magneticDeclination;
            UpdateNavaid();
        }

        private void OnEnable()
        {
            navaidUpdateIntervalOffset = Random.Range(0, navaidUpdateInterval);
        }

        public override void PostLateUpdate()
        {
            if ((Time.renderedFrameCount + navaidUpdateIntervalOffset) % navaidUpdateInterval == 0) UpdateNavaid();

            deltaTime = Time.deltaTime;

            var position = transform.position;
            var course = navaidSelector.Course;
            var relativePosition = navaidPosition - position;

            cdiAnimator.SetFloat(courseFloatParameter, course / 360.0f, dampTime, deltaTime);

            if (isVOR)
            {
                var toFrom = GetVORToFrom(GetCourseVector(course), relativePosition);
                cdiAnimator.SetFloat(toFromFloatParameeter, Remap11to01(toFrom), dampTime, deltaTime);
                cdiAnimator.SetFloat(
                    courseDeviationFloatParameter,
                    Mathf.Approximately(toFrom, 0.0f)
                        ? 0.5f
                        : Remap11to01(GetCourseDeviation(relativePosition, course) / 10.0f),
                    dampTime,
                    deltaTime
                );

                cdiAnimator.SetBool(glideSlopeBoolParameter, false);
                cdiAnimator.SetFloat(glideSlopeDeviationFloatParameter, 0.0f, dampTime, deltaTime);
            }
            else if (isILS)
            {
                var isBackCourse = IsLocalizerBackCourse(relativePosition);
                var courseDeviation = GetCourseDeviation(relativePosition, course);
                var maxRange = isBackCourse ? localizerMaxBackRange : localizerMaxWideRange;
                var distance = relativePosition.magnitude;
                var localizerCaptured = distance < maxRange && Mathf.Abs(courseDeviation) < (isBackCourse || distance > localizerMaxWideRange ? 10.0f : 35.0f);

                cdiAnimator.SetFloat(toFromFloatParameeter, localizerCaptured ? 1.0f : 0.5f, dampTime, deltaTime);
                cdiAnimator.SetFloat(
                    courseDeviationFloatParameter,
                    localizerCaptured
                        ? Remap11to01(GetCourseDeviation(relativePosition, course) / 3.0f)
                        : 0.5f,
                    dampTime,
                    deltaTime
                );

                var glideSlopeDeviation = GetGlideSlopeDeviation(relativePosition);
                var glideSlopeCaptured = IsBetween(glideSlopeDeviation, glideSlopeMinDeviation, glideSlopeMaxDeviation);
                cdiAnimator.SetBool(glideSlopeBoolParameter, glideSlopeCaptured);
                cdiAnimator.SetFloat(
                    glideSlopeDeviationFloatParameter,
                    glideSlopeCaptured
                        ? Remap11to01(glideSlopeDeviation / 0.7f)
                        : 0.0f,
                    dampTime,
                    deltaTime
                );
            }
            else
            {
                cdiAnimator.SetFloat(toFromFloatParameeter, 0.5f, dampTime, deltaTime);
                cdiAnimator.SetFloat(courseDeviationFloatParameter, 0.0f, dampTime, deltaTime);
                cdiAnimator.SetBool(glideSlopeBoolParameter, false);
                cdiAnimator.SetFloat(glideSlopeDeviationFloatParameter, 0.0f, dampTime, deltaTime);
            }
        }

        private float GetLocalizerRange(float distance)
        {
            return Lerp3(35.0f, 10.0f, -10.0f, 10.0f, 18.0f, 24.0f, distance / 1852.0f);
        }

        private float GetCourseDeviation(Vector3 relativePosition, float course)
        {
            var courseVector = isILS ? -navaidForward : Quaternion.AngleAxis(course, Vector3.up) * Vector3.forward;
            var courseCross = Vector3.Cross(Vector3.up, courseVector).normalized;
            var projected = Vector3.ProjectOnPlane(relativePosition, Vector3.up).normalized;
            return Mathf.Rad2Deg * Mathf.Atan(Vector3.Dot(projected, courseCross));
        }

        private void UpdateNavaid()
        {
            var index = navaidSelector.Index;
            tuned = index >= 0;

            var database = navaidSelector.database;
            if (!database) return;

            navaidTransform = tuned ? database.transforms[index] : null;
            navaidPosition = tuned ? navaidTransform.position : Vector3.zero;
            navaidForward = tuned ? navaidTransform.forward : Vector3.zero;

            isVOR = tuned && database._IsVOR(index);
            isILS = tuned && database._IsILS(index);

            glideSlopeTransform = isILS ? database.glideSlopeTransforms[index] : null;
            glideSlopePosition = isILS ? glideSlopeTransform.position : Vector3.zero;
            glideSlopeForward = isILS ? glideSlopeTransform.forward : Vector3.zero;
            glideSlopeUp = isILS ? glideSlopeTransform.up : Vector3.zero;
            glideSlopeRight = isILS ? glideSlopeTransform.right : Vector3.zero;
            glideSlopeAngle = isILS ? Mathf.Atan(glideSlopeForward.y) * Mathf.Rad2Deg : 0.0f;
            glideSlopeMinDeviation = glideSlopeAngle * 0.45f - glideSlopeAngle;
            glideSlopeMaxDeviation = glideSlopeAngle * 1.75f - glideSlopeAngle;
        }

        private float Remap01(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        private float Remap11to01(float value)
        {
            return (value + 1.0f) * 0.5f;
        }

        private float Lerp3(float v0, float v1, float v2, float t0, float t1, float t2, float t)
        {
            return t < t1 ? Mathf.Lerp(v0, v1, Remap01(t, t0, t1)) : Mathf.Lerp(v1, v2, Remap01(t, t1, t2));
        }

        private Vector3 GetCourseVector(float course)
        {
            return isILS ? -navaidForward : Quaternion.AngleAxis(course, Vector3.up) * Vector3.forward;
        }

        private bool IsBetween(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        private float GetVORToFrom(Vector3 courseVector, Vector3 relativePosition)
        {
            if (!IsBetween(Vector3.Angle(Vector3.up, relativePosition), 45, 90)) return 0.0f;
            return -Mathf.Sign(Vector3.Dot(courseVector, relativePosition));
        }

        private bool InLocalizerRange(Vector3 relativePosition)
        {
            return IsBetween(Vector3.Dot(navaidForward, relativePosition) / 1852.0f, -10.0f, 18.0f);
        }

        private bool IsLocalizerBackCourse(Vector3 relativePosition)
        {
            return Vector3.Dot(navaidForward, relativePosition) > 0;
        }

        private float GetGlideSlopeDeviation(Vector3 relativePosition)
        {
            return Vector3.SignedAngle(Vector3.ProjectOnPlane(relativePosition, glideSlopeRight), glideSlopeForward, glideSlopeRight);
        }

        private const float vorMaxRange = 200 * 1852.0f;
        private const float localizerMaxRange = 18 * 1852.0f;
        private const float localizerMaxWideRange = 10 * 1852.0f;
        private const float localizerMaxBackRange = 10 * 1852.0f;
        private const float glideSlopeMaxRange = 10 * 1852.0f;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            this.UpdateProxy();
            if (!tuned) return;

            Gizmos.color = Color.white;
            Gizmos.DrawRay(navaidPosition, Vector3.up * 1000.0f);

            var course = navaidSelector.Course;
            var position = transform.position;
            var relativePosition = position - navaidPosition;
            var courseVector = GetCourseVector(course);

            if (isVOR)
            {
                var toFrom = GetVORToFrom(courseVector, relativePosition);
                Gizmos.color = Mathf.Approximately(toFrom, 0.0f) ? Color.red : toFrom > 0 ? Color.green : Color.blue;
                Gizmos.DrawRay(navaidPosition, courseVector * vorMaxRange);
                Gizmos.DrawRay(navaidPosition, -courseVector * vorMaxRange);

                var courseDeviation = GetCourseDeviation(relativePosition, course);
                var clampedCourseDeviation = Mathf.Clamp(courseDeviation, -10.0f, 10.0f);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(navaidPosition, Quaternion.AngleAxis(clampedCourseDeviation, Vector3.up) * courseVector * vorMaxRange);
                Gizmos.color = Color.green;
                Gizmos.DrawRay(navaidPosition, Quaternion.AngleAxis(-clampedCourseDeviation, Vector3.up) * courseVector * -vorMaxRange);
            }

            if (isILS)
            {
                var localizerGizmoOrigin = navaidPosition + Vector3.Project(relativePosition, Vector3.up);
                Gizmos.color = Color.white;

            var distance = relativePosition.magnitude;
                var isBackCourse = IsLocalizerBackCourse(relativePosition);
                var courseDeviation = GetCourseDeviation(relativePosition, course);
                var maxRange = isBackCourse ? localizerMaxBackRange : localizerMaxWideRange;
                var localizerCaptured = distance < maxRange && Mathf.Abs(courseDeviation) < (isBackCourse || distance > localizerMaxWideRange ? 10.0f : 35.0f);

                Gizmos.color = localizerCaptured ? Color.white : Color.red;
                Gizmos.DrawRay(localizerGizmoOrigin, courseVector * localizerMaxRange);
                Gizmos.DrawRay(localizerGizmoOrigin, -courseVector * localizerMaxBackRange);

                if (localizerCaptured)
                {
                    var clampedCourseDeviation = Mathf.Clamp(courseDeviation, -3.0f, 3.0f);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(localizerGizmoOrigin, Quaternion.AngleAxis(clampedCourseDeviation, Vector3.up) * courseVector * localizerMaxRange);
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(localizerGizmoOrigin, Quaternion.AngleAxis(-clampedCourseDeviation, Vector3.up) * courseVector * -localizerMaxRange);
                }

                var glideSlopeGizmoOrigin = glideSlopePosition + Vector3.Project(relativePosition, glideSlopeRight);

                var glideSlopeDeviation = GetGlideSlopeDeviation(relativePosition);
                var glideSlopeCaptured = IsBetween(glideSlopeDeviation, glideSlopeMinDeviation, glideSlopeMaxDeviation);
                var clampedGlideSlopeDeviation = Mathf.Clamp(glideSlopeDeviation, -0.7f, 0.7f);

                Gizmos.color = glideSlopeCaptured ? Color.white : Color.red;
                Gizmos.DrawRay(glideSlopeGizmoOrigin, glideSlopeForward * 10 * 1852.0f);
                if (glideSlopeCaptured)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(glideSlopeGizmoOrigin, Quaternion.AngleAxis(clampedGlideSlopeDeviation, glideSlopeRight) * glideSlopeForward * glideSlopeMaxRange);
                }
            }
        }
#endif
    }
}
