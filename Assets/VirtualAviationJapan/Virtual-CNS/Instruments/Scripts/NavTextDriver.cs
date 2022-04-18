
using System.Diagnostics.PerformanceData;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(90)] // After Nav Selector, Before Instruments
    public class NavTextDriver : UdonSharpBehaviour
    {
        public TextMeshProUGUI[] navTexts;
        public TextMeshProUGUI toFromText;

        public float updateInterval = 0.2f;


        private Transform origin;
        private float selectedCourse;
        private string[] navaidIdentities;
        private Transform[] navaidTransforms, glideSlopeTransforms, dmeTransforms;
        private bool[] hasDMEs;
        private bool[] hasCourses;
        private string[] navaidIdentityFormats;
        private NavSelector[] navSelectors;
        private int index;
        private bool initialized;
        private readonly string STRING_TO = "To", STRING_FROM = "From";

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
#if UNITY_ANDROID
            updateInterval *= 2;
#endif
        }

        private void OnEnable()
        {
            if (initialized) SendCustomEventDelayedSeconds(nameof(_ThinUpdate), Mathf.Max(Random.value * updateInterval, Time.fixedDeltaTime));
        }

        public void _NavReady() => SendCustomEventDelayedSeconds(nameof(_Initialize), 1 + Random.value * updateInterval);

        public void _Initialize()
        {
            var rootObject = GetRootObject();
            origin = rootObject.transform;

            var refSelector = GetComponentInParent<NavSelector>();
            navSelectors = FindNavSelectors(refSelector);
            navaidIdentities = new string[navSelectors.Length];
            navaidTransforms = new Transform[navSelectors.Length];
            glideSlopeTransforms = new Transform[navSelectors.Length];
            dmeTransforms = new Transform[navSelectors.Length];
            hasDMEs = new bool[navSelectors.Length];
            hasCourses = new bool[navSelectors.Length];

            foreach (var navSelector in navSelectors)
            {
                if (navSelector != null) navSelector._Subscribe(this);
            }

            navaidIdentityFormats = new string[navTexts.Length];
            for (var i = 0; i < navaidIdentityFormats.Length; i++)
            {
                var navaidIdentity = navTexts[i];
                if (navaidIdentity == null) continue;
                navaidIdentityFormats[i] = navaidIdentity.text;
                navaidIdentity.text = null;
            }

            initialized = true;
            _NavChanged();
            _ThinUpdate();
        }

        public void _ThinUpdate()
        {
            if (!gameObject.activeInHierarchy) return;

            var position = origin.position;

            index = (index + 1) % navSelectors.Length;
            UpdateNavText(position, navaidIdentities[index], dmeTransforms[index], navTexts[index], navaidIdentityFormats[index], hasDMEs[index]);

            if (index == 0 && toFromText)
            {
                var vorTransform = navaidTransforms[index];
                if (vorTransform)
                {
                    var relative = vorTransform.position - position;
                    var gsTransform = glideSlopeTransforms[index];
                    var isLocalizer = gsTransform != null;
                    var courseVector = isLocalizer ? -vorTransform.forward : Quaternion.AngleAxis(selectedCourse, Vector3.up) * Vector3.forward;
                    var toFrom = Mathf.Sign(Vector3.Dot(courseVector, isLocalizer ? (gsTransform.position - position) : relative));

                    ((Component)toFromText).gameObject.SetActive(!isLocalizer && hasCourses[index]);
                    if (!isLocalizer) toFromText.text = toFrom >= 0 ? STRING_TO : STRING_FROM;
                }
            }

            SendCustomEventDelayedSeconds(nameof(_ThinUpdate), updateInterval);
        }

        public void _NavChanged()
        {
            if (!initialized) return;

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

                UpdateNavText(position, navaidIdentities[i], dmeTransforms[i], navTexts[i], navaidIdentityFormats[i], hasDMEs[i]);

                if (i == 0)
                {
                    selectedCourse = navSelector.Course;
                }
            }
        }

        private void UpdateText(TextMeshProUGUI textMesh, string value)
        {
            if (textMesh == null) return;
            textMesh.text = value;
        }

        private void UpdateNavText(Vector3 position, string identity, Transform dmeTransform, TextMeshProUGUI navaidIdentity, string format, bool hasDME)
        {
            if (navaidIdentity == null || format == null) return;
            navaidIdentity.text = string.Format(format, identity, hasDME ? (Vector3.Distance(position, dmeTransform.position) * 0.000621371f).ToString("#0.0") : "--.-", selectedCourse);
        }
    }
}
