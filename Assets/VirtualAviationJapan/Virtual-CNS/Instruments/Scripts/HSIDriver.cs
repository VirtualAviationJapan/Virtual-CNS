
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [DefaultExecutionOrder(90)] // After Nav Selector, Before Instruments
    public class HSIDriver : UdonSharpBehaviour
    {
        public float magneticDeclination = 0.0f;
        public Transform heading;
        public TextMeshProUGUI headingText;
        public Transform courseIndicator;
        public Transform courseDiviationIndicator;
        public TextMeshProUGUI[] navTexts;
        public TextMeshProUGUI toFromText;
        public float cdiMaxScale = 128;

        public Transform glideSlopeIndicator;
        public GameObject glideSlopeIndicatorParent;
        public float glideSlopeMaxScale = 128;

        public int updateInterval = 9;

        private Transform origin;
        private float selectedCourse;
        private string[] navaidIdentities;
        private Transform[] navaidTransforms, glideSlopeTransforms, dmeTransforms;
        private bool[] hasDMEs;
        private bool[] hasCourses;
        private string[] navaidIdentityFormats;
        private NavSelector[] navSelectors;

        private readonly string STRING_TO = "To", STRING_FROM = "From";

        private GameObject GetRootObject()
        {
            var rigidbody = GetComponentInParent<Rigidbody>();
            return rigidbody != null ? rigidbody.gameObject : gameObject;
        }

        private NavSelector[] FindNavSelectors(GameObject rootObject, NavSelector refSelector)
        {
            var foundNavSelectors = rootObject.GetComponentsInChildren<NavSelector>(true);
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

            navaidIdentityFormats = new string[navTexts.Length];
            for (var i = 0; i < navaidIdentityFormats.Length; i++)
            {
                var navaidIdentity = navTexts[i];
                if (navaidIdentity == null) continue;
                navaidIdentityFormats[i] = navaidIdentity.text;
                navaidIdentity.text = null;
            }

            var refSelector = GetComponentInParent<NavSelector>();
            navSelectors = FindNavSelectors(rootObject, refSelector);
            navaidIdentities = new string[navSelectors.Length];
            navaidTransforms = new Transform[navSelectors.Length];
            glideSlopeTransforms = new Transform[navSelectors.Length];
            dmeTransforms = new Transform[navSelectors.Length];
            hasDMEs = new bool[navSelectors.Length];
            hasCourses = new bool[navSelectors.Length];

            SendCustomEventDelayedFrames(nameof(_PostStart), 1);

            var navaidDatabaseObj = GameObject.Find("NavaidDatabase");
            if (navaidDatabaseObj) magneticDeclination = (float)((UdonBehaviour)navaidDatabaseObj.GetComponent(typeof(UdonBehaviour))).GetProgramVariable("magneticDeclination");

#if UNITY_ANDROID
            updateInterval *= 2;
#endif
        }

        private int updateIntervalOffset;
        private void OnEnable()
        {
            updateIntervalOffset = UnityEngine.Random.Range(0, updateInterval);
        }

        public void _PostStart()
        {
            foreach (var navSelector in navSelectors)
            {
                if (navSelector != null) navSelector._Subscribe(this);
            }
        }

        private void LateUpdate()
        {
            var frame = Time.frameCount + updateIntervalOffset;
            if (frame % updateInterval != 0) return;
            var position = origin.position;

            var forward = origin.forward;
            var headingAngle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg + magneticDeclination;

            if (heading != null) heading.localRotation = Quaternion.AngleAxis(headingAngle, Vector3.forward);
            if (headingText != null) headingText.text = ((headingAngle % 360 + 360) % 360).ToString("000");

            if (hasCourses[0]) CDI_Update(position, navaidTransforms[0], glideSlopeTransforms[0], selectedCourse, courseIndicator, courseDiviationIndicator, toFromText);
            GSI_Update(position, navaidTransforms[0], glideSlopeTransforms[0]);

            var i = frame / updateInterval % navSelectors.Length;
            UpdateNavText(position, navaidIdentities[i], dmeTransforms[i], navTexts[i], navaidIdentityFormats[i], hasDMEs[i]);
        }

        public void _NavChanged()
        {
            var position = origin.position;

            for (var i = 0; i < navSelectors.Length; i++)
            {
                var navSelector = navSelectors[i];
                if (navSelector == null) continue;

                navaidIdentities[i] = navSelector.Identity;
                navaidTransforms[i] = navSelector.NavaidTransform;
                glideSlopeTransforms[i] = navSelector.GlideSlopeTransform;
                dmeTransforms[i] = navSelector.DMETransform;
                hasDMEs[i] = navSelector.HasDME;
                var hasCourse = navSelector.IsVOR || navSelector.IsILS;
                hasCourses[i] = hasCourse;

                if (i == 0)
                {
                    selectedCourse = navSelector.Course;

                    if (courseIndicator && courseIndicator.gameObject.activeSelf != hasCourse)
                    {
                        courseIndicator.gameObject.SetActive(hasCourse);
                    }
                }

                UpdateNavText(position, navaidIdentities[i], dmeTransforms[i], navTexts[i], navaidIdentityFormats[i], hasDMEs[i]);
            }
        }

        private void UpdateText(TextMeshProUGUI textMesh, string value)
        {
            if (textMesh == null) return;
            textMesh.text = value;
        }

        private void CDI_Update(Vector3 position, Transform vorTransform, Transform gsTransform, float course, Transform courseIndicator, Transform courseDiviationIndiator, TextMeshProUGUI toFromText)
        {
            if (vorTransform == null) return;

            var relative = vorTransform.position - position;

            var isLocalizer = gsTransform != null;
            var courseVector = isLocalizer ? -vorTransform.forward : Quaternion.AngleAxis(course, Vector3.up) * Vector3.forward;
            var courseCross = Vector3.Cross(Vector3.up, courseVector).normalized;
            var toFrom = Mathf.Sign(Vector3.Dot(courseVector, isLocalizer ? (gsTransform.position - position) : relative));
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

            if (toFromText != null)
            {
                ((Component)toFromText).gameObject.SetActive(!isLocalizer);
                if (!isLocalizer) toFromText.text = toFrom >= 0 ? STRING_TO : STRING_FROM;
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

        private void UpdateNavText(Vector3 position, string identity, Transform dmeTransform, TextMeshProUGUI navaidIdentity, string format, bool hasDME)
        {
            if (navaidIdentity == null || format == null) return;
            navaidIdentity.text = string.Format(format, identity, hasDME ? (Vector3.Distance(position, dmeTransform.position) * 0.000621371f).ToString("#0.0") : "--.-", selectedCourse);
        }
    }
}
