
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class HIDriver : UdonSharpBehaviour
    {
        public float magneticDeclination = 0.0f;
        public Transform heading;
        public TextMeshProUGUI headingText;

        public Image turnIndicator;
        public float turnIndicatorScale = 10;

        public float updateInterval = 0.2f;

        private Transform origin;

        private float prevHeadingAngle;
        private float prevTime;

        private GameObject GetRootObject()
        {
            var rigidbody = GetComponentInParent<Rigidbody>();
            return rigidbody != null ? rigidbody.gameObject : gameObject;
        }

        private void Start()
        {
            var rootObject = GetRootObject();
            origin = rootObject.transform;

            var navaidDatabaseObj = GameObject.Find("NavaidDatabase");
            if (navaidDatabaseObj) magneticDeclination = (float)((UdonBehaviour)navaidDatabaseObj.GetComponent(typeof(UdonBehaviour))).GetProgramVariable("magneticDeclination");

#if UNITY_ANDROID
            updateInterval *= 2;
#endif
        }

        private void OnEnable()
        {
            SendCustomEventDelayedSeconds(nameof(_ThinUpdate), Mathf.Max(UnityEngine.Random.value * updateInterval, Time.fixedDeltaTime * 2));
        }

        public void _ThinUpdate()
        {
            if (!gameObject.activeInHierarchy) return;

            var position = origin.position;
            var time = Time.time;
            var deltaTime = time - prevTime;
            prevTime = time;

            var forward = origin.forward;
            var headingAngle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg + magneticDeclination;

            if (heading != null) heading.localRotation = Quaternion.AngleAxis(headingAngle, Vector3.forward);
            if (headingText != null) headingText.text = ((headingAngle % 360 + 360) % 360).ToString("000");

            if (turnIndicator != null)
            {
                var turnRate = Mathf.DeltaAngle(headingAngle, prevHeadingAngle) * turnIndicatorScale / deltaTime;
                turnIndicator.fillAmount = Mathf.Clamp01(Mathf.Abs(turnRate) / 360.0f);
                turnIndicator.fillClockwise = turnRate < 0;
                prevHeadingAngle = headingAngle;
            }

            SendCustomEventDelayedSeconds(nameof(_ThinUpdate), updateInterval);
        }
    }
}
