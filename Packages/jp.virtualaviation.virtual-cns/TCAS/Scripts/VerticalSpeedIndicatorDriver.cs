
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VerticalSpeedIndicatorDriver : UdonSharpBehaviour
    {
        private const float FPM = 196.85f;

        public Transform verticalSpeedIndicatorTransform;
        public float maxVerticalSpeed = 6000;
        public float maxAngle = 170;
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
            angleScaler = 1.0f / Mathf.Log(2, 2);
        }

        private void Update()
        {
            if ((Time.frameCount + updateOffset) % updateInterval != 0) return;

            var time = Time.time;
            var deltaTime = time - lastUpdateTime;

            var vehicleAltitude = vehicleTransform.position.y;

            verticalSpeed = Mathf.Lerp(verticalSpeed, (vehicleAltitude - prevAltitude) * FPM / deltaTime, deltaTime / smoothing);
            verticalSpeedIndicatorTransform.localRotation = Quaternion.AngleAxis(ApplyLog2Curve(Mathf.Clamp(verticalSpeed / maxVerticalSpeed, -1, 1)) * maxAngle, -Vector3.forward);

            lastUpdateTime = time;
            prevAltitude = vehicleAltitude;
        }

        private readonly float Log2Scaler = 1.0f / Mathf.Log(2, 2);
        private float ApplyLog2Curve(float value)
        {
            return Mathf.Log(Mathf.Abs(value) + 1, 2) * Log2Scaler * Mathf.Sign(value);
        }
    }
}
