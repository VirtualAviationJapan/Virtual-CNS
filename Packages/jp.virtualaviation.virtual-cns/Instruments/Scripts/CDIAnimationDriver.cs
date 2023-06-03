
using UdonSharp;
using UdonToolkit;
using UnityEngine;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace VirtualCNS
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
        [Range(-1.0f, 1.0f)] public float cdOff = 0.0f;
        [Range(-1.0f, 1.0f)] public float gsOff = 0.0f;
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

            cdiAnimator.SetFloat(courseFloatParameter, course / 360.0f);

            if (isVOR)
            {
                var courceVector = NavigationMath.CourseToVector(course, magneticDeclination);
                var toFrom = NavigationMath.GetVORToFrom(courceVector, relativePosition);
                cdiAnimator.SetFloat(toFromFloatParameeter, NavigationMath.Remap11to01(toFrom), dampTime, deltaTime);
                cdiAnimator.SetFloat(
                    courseDeviationFloatParameter,
                    NavigationMath.Remap11to01(Mathf.Approximately(toFrom, 0.0f) ? cdOff : NavigationMath.GetCourseDeviation(relativePosition, courceVector) / 10.0f),
                    dampTime,
                    deltaTime
                );

                cdiAnimator.SetBool(glideSlopeBoolParameter, false);
                cdiAnimator.SetFloat(glideSlopeDeviationFloatParameter, NavigationMath.Remap11to01(gsOff), dampTime, deltaTime);
            }
            else if (isILS)
            {
                var courceVector = -navaidForward;
                var isBackCourse = NavigationMath.IsBehind(navaidForward, relativePosition);
                var courseDeviation = NavigationMath.GetCourseDeviation(relativePosition, courceVector);
                var maxRange = isBackCourse ? NavigationMath.LocalizerMaxBackRange : NavigationMath.LocalizerMaxWideRange;
                var distance = relativePosition.magnitude;
                var localizerCaptured = distance < maxRange && Mathf.Abs(courseDeviation) < (isBackCourse || distance > NavigationMath.LocalizerMaxWideRange ? 10.0f : 35.0f);

                cdiAnimator.SetFloat(toFromFloatParameeter, localizerCaptured ? 1.0f : 0.5f, dampTime, deltaTime);
                cdiAnimator.SetFloat(
                    courseDeviationFloatParameter,
                    NavigationMath.Remap11to01(localizerCaptured ? courseDeviation / 3.0f : cdOff),
                    dampTime,
                    deltaTime
                );

                var glideSlopeDeviation = NavigationMath.GetGlideslopeDeviation(glideSlopeForward, glideSlopeRight, glideSlopePosition - position);
                var glideSlopeCaptured =  Mathf.Abs(courseDeviation) < 8.0f && NavigationMath.IsBetween(glideSlopeDeviation, glideSlopeMinDeviation, glideSlopeMaxDeviation);
                cdiAnimator.SetBool(glideSlopeBoolParameter, glideSlopeCaptured);
                cdiAnimator.SetFloat(
                    glideSlopeDeviationFloatParameter,
                    NavigationMath.Remap11to01(glideSlopeCaptured ? glideSlopeDeviation / 0.7f : gsOff),
                    dampTime,
                    deltaTime
                );
            }
            else
            {
                cdiAnimator.SetFloat(toFromFloatParameeter, 0.5f, dampTime, deltaTime);
                cdiAnimator.SetFloat(courseDeviationFloatParameter, NavigationMath.Remap11to01(cdOff), dampTime, deltaTime);
                cdiAnimator.SetBool(glideSlopeBoolParameter, false);
                cdiAnimator.SetFloat(glideSlopeDeviationFloatParameter, NavigationMath.Remap11to01(gsOff), dampTime, deltaTime);
            }
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
    }
}
