
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace VirtualCNS
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RadioMagneticIndicator : UdonSharpBehaviour
    {

        public int updateInterval = 10;
        private int intervalOffset;
        private Transform origin;
        private Transform target;
        private Image[] images;
        private void Start()
        {
            intervalOffset = UnityEngine.Random.Range(0, updateInterval);

            var rigidbody = GetComponentInParent<Rigidbody>();
            origin = rigidbody ? rigidbody.transform : transform;

            images = GetComponentsInChildren<Image>(true);

            _SetTarget(null, Color.black);
        }

        private void Update()
        {
            if ((Time.frameCount + intervalOffset) % updateInterval != 0 || target == null) return;
            var bearing = Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(origin.position - target.position, Vector3.up), Vector3.up);
            transform.localRotation = Quaternion.AngleAxis(bearing, Vector3.forward);
        }

        public void _SetTarget(Transform value, Color color)
        {
            target = value;
            gameObject.SetActive(value != null);
            foreach (var image in images) image.color = color;
        }
    }
}
