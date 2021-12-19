using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class HeadingIndicator : UdonSharpBehaviour
    {
        public float updateInterval = 0.25f;

        private Transform origin;
        private float magneticDeclination;

        private void Start()
        {
            var vehicleRigidbody = GetComponentInParent<Rigidbody>();
            if (vehicleRigidbody) origin = vehicleRigidbody.transform;
            if (origin == null) origin = transform.parent;

            var navaidDatabaseObj = GameObject.Find("NavaidDatabase");
            if (navaidDatabaseObj) magneticDeclination = (float)((UdonBehaviour)navaidDatabaseObj.GetComponent(typeof(UdonBehaviour))).GetProgramVariable("magneticDeclination");
        }

        private void Update()
        {
            var heading = Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.up) + magneticDeclination;

            transform.localRotation = Quaternion.AngleAxis(heading, Vector3.forward);

            enabled = false;
            SendCustomEventDelayedSeconds(nameof(_ThinUpdate), updateInterval * Random.Range(0.9f, 1.1f));
        }

        public void _ThinUpdate()
        {
            enabled = true;
        }
    }
}
