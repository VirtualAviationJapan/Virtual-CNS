
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VerticalSpeedIndicatorDriver : UdonSharpBehaviour
    {
        private const float FPM = 196.85f;

        public Transform verticalSpeedIndicatorTransform;
        public float smoothing = 1;
        public int updateInterval = 9;
        private Rigidbody vehicleRigidbody;
        private Transform vehicleTransform;
        private int updateOffset;
        private float lastUpdateTime;
        private float prevAltitude;
        private float verticalSpeed;
        private float angleScaler;

        private void Start()
        {
            updateOffset = UnityEngine.Random.Range(0, updateInterval);
            vehicleRigidbody = GetComponentInParent<Rigidbody>();
            vehicleTransform = vehicleRigidbody ? vehicleRigidbody.transform : transform;
            angleScaler = 170.0f / Mathf.Log(6 + 1, 2);
        }

        private void Update()
        {
            if ((Time.frameCount + updateOffset) % updateInterval != 0) return;

            var time = Time.time;
            var deltaTime = time - lastUpdateTime;

            var vehicleAltitude = vehicleTransform.position.y;

            verticalSpeed = Mathf.Lerp(verticalSpeed, (vehicleAltitude - prevAltitude) * FPM / deltaTime, deltaTime / smoothing);
            verticalSpeedIndicatorTransform.localRotation = Quaternion.AngleAxis(Mathf.Clamp(Mathf.Log(Mathf.Abs(verticalSpeed / 1000) + 1, 2) * angleScaler, 0, 170) * Mathf.Sign(verticalSpeed), -Vector3.forward);

            lastUpdateTime = time;
            prevAltitude = vehicleAltitude;
        }
    }
}
