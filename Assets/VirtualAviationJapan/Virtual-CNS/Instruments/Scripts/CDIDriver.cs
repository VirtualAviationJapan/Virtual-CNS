
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CDIDriver : UdonSharpBehaviour
    {
        public float magneticDeclination = 0.0f;
        public Transform courseIndicator;
        public Transform courseDiviationIndicator;
        public float cdiMaxScale = 128;

        public Transform glideSlopeIndicator;
        public GameObject glideSlopeIndicatorParent;
        public float glideSlopeMaxScale = 128;
        public float updateInterval = 0.2f;

        private Transform origin;
        private float selectedCourse;
        private Transform navaidTransform, glideSlopeTransform, dmeTransform;
        private bool hasDME, hasCourse;
        private NavSelector navSelector;
        private bool initialized;

        private GameObject GetRootObject()
        {
            var rigidbody = GetComponentInParent<Rigidbody>();
            return rigidbody != null ? rigidbody.gameObject : gameObject;
        }

        private NavSelector[] FindNavSelectors(NavSelector refSelector)
        {
            var foundNavSelectors = GetRootObject().GetComponentsInChildren<NavSelector>(true);
            var result = new NavSelector[foundNavSelectors.Length + 1];
            result[0] = refSelector;
            for (var i = 0; i < foundNavSelectors.Length; i++)
            {
                var navSelector = foundNavSelectors[i];
                if (navSelector == null) continue;

                result[i + 1] = navSelector;
            }
            return result;
        }

        private void Start()
        {
            var rootObject = GetRootObject();
            origin = rootObject.transform;

            navSelector = GetComponentInParent<NavSelector>();

            var navaidDatabaseObj = GameObject.Find("NavaidDatabase");
            if (navaidDatabaseObj) magneticDeclination = (float)((UdonBehaviour)navaidDatabaseObj.GetComponent(typeof(UdonBehaviour))).GetProgramVariable("magneticDeclination");

            initialized = true;
            SendCustomEventDelayedSeconds(nameof(_ThinUpdate), Mathf.Max(Random.value * updateInterval, Time.fixedDeltaTime));
#if UNITY_ANDROID
            updateInterval *= 2;
#endif
        }

        private void OnEnable()
        {
            if (initialized) SendCustomEventDelayedSeconds(nameof(_ThinUpdate), Mathf.Max(Random.value * updateInterval, Time.fixedDeltaTime));
        }

        public void _NavReady()
        {
            navSelector._Subscribe(this);
        }

        public void _ThinUpdate()
        {
            if (!initialized || !gameObject.activeInHierarchy) return;

            var position = origin.position;

            var forward = origin.forward;
            var headingAngle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg + magneticDeclination;

            if (hasCourse) CDI_Update(position, navaidTransform, glideSlopeTransform, selectedCourse, courseIndicator, courseDiviationIndicator);
            GSI_Update(position, navaidTransform, glideSlopeTransform);

            SendCustomEventDelayedSeconds(nameof(_ThinUpdate), updateInterval);
        }

        public void _NavChanged()
        {
            if (!initialized) return;

            navaidTransform = navSelector.NavaidTransform;
            glideSlopeTransform = navSelector.GlideSlopeTransform;
            dmeTransform = navSelector.DMETransform;
            hasDME = navSelector.HasDME;
            hasCourse = navSelector.IsVOR || navSelector.IsILS;

            selectedCourse = navSelector.Course;

            if (courseIndicator && courseIndicator.gameObject.activeSelf != hasCourse)
            {
                courseIndicator.gameObject.SetActive(hasCourse);
            }
        }

        private void CDI_Update(Vector3 position, Transform vorTransform, Transform gsTransform, float course, Transform courseIndicator, Transform courseDiviationIndiator)
        {
            if (vorTransform == null) return;

            var relative = vorTransform.position - position;

            var isLocalizer = gsTransform != null;
            var courseVector = isLocalizer ? -vorTransform.forward : Quaternion.AngleAxis(course, Vector3.up) * Vector3.forward;
            var courseCross = Vector3.Cross(Vector3.up, courseVector).normalized;
            var clamp = isLocalizer ? 3 : 10;
            var scale = cdiMaxScale / clamp;

            if (courseIndicator != null)
            {
                courseIndicator.localRotation = isLocalizer ? Quaternion.AngleAxis(180 - vorTransform.eulerAngles.y, Vector3.forward) : Quaternion.AngleAxis(-course, Vector3.forward);
            }

            if (courseDiviationIndiator != null)
            {
                var projected = Vector3.ProjectOnPlane(relative, Vector3.up).normalized;
                var courseDiviation = Mathf.Rad2Deg * Mathf.Atan(Vector3.Dot(projected, courseCross));
                courseDiviationIndiator.localPosition = Vector3.right * Mathf.Clamp(courseDiviation, -clamp, clamp) * scale;
            }
        }

        private void GSI_Update(Vector3 position, Transform localizerTransform, Transform glideSlopeTransform)
        {
            if (glideSlopeIndicator == null || glideSlopeIndicatorParent == null) return;

            if (localizerTransform == null || glideSlopeTransform == null || Vector3.Dot(-localizerTransform.forward, glideSlopeTransform.position - position) < 0)
            {
                glideSlopeIndicatorParent.SetActive(false);
            }
            else
            {
                glideSlopeIndicatorParent.SetActive(true);
                var clamp = 10.0f;
                var scale = glideSlopeMaxScale / clamp;
                var angle = -Mathf.Atan(Vector3.Dot(Vector3.ProjectOnPlane(position - glideSlopeTransform.position, glideSlopeTransform.forward).normalized, glideSlopeTransform.up)) * Mathf.Rad2Deg;
                glideSlopeIndicator.localPosition = Vector3.up * scale * Mathf.Clamp(angle, -clamp, clamp);
            }
        }
    }
}
