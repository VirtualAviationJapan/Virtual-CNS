
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
        public float magneticDeclination = 0;
        public NavSelector navSelector;

        private int intervalOffset;
        private Transform origin;
        private Transform target;
        private Image[] images;
        private Color color = Color.white;
        private void Start()
        {
            intervalOffset = UnityEngine.Random.Range(0, updateInterval);

            var rigidbody = GetComponentInParent<Rigidbody>();
            origin = rigidbody ? rigidbody.transform : transform;

            images = GetComponentsInChildren<Image>(true);
            if (images.Length > 0) color = images[0].color;

            _SetTarget(null, Color.black);
            SendCustomEventDelayedSeconds(nameof(_PostStart), 0.5f);
        }

        public void _PostStart()
        {
            if (!navSelector) return;

            navSelector._Subscribe(this);
            magneticDeclination = navSelector.database.magneticDeclination;
        }

        public void _NavChanged()
        {
            if (!navSelector) return;
            target = navSelector.IsVOR ? navSelector.NavaidTransform : null;
            _SetTarget(target, target ? color : Color.black);
        }

        private void Update()
        {
            if ((Time.frameCount + intervalOffset) % updateInterval != 0 || target == null) return;
            var bearing = Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(target.position - origin.position, Vector3.up), Vector3.up) + magneticDeclination;
            transform.localRotation = Quaternion.AngleAxis(-bearing, Vector3.forward);
        }

        public void _SetTarget(Transform value, Color color)
        {
            target = value;
            gameObject.SetActive(value != null);
            foreach (var image in images) image.color = color;
        }
    }
}
