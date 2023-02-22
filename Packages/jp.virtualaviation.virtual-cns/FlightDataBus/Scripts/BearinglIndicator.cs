using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
{

    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BearinglIndicator : FlightDataBusClient
    {
        public int id = 1;
        public Vector3 axis = Vector3.forward;
        private FlightDataFloatValueId radialId;
        private FlightDataBoolValueId tunedId;

        protected override void OnStart()
        {
            var offset = id - 1;
            radialId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Radial, offset);
            tunedId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1Tuned, offset);
            _Sbuscribe(tunedId);
        }

        public override void _OnBoolValueChanged()
        {
            var tuned = _Read(tunedId);
            if (gameObject.activeSelf != tuned) gameObject.SetActive(true);
        }

        private void Update()
        {
            transform.localRotation = Quaternion.AngleAxis(_Read(radialId) + 180, axis);
        }
    }
}
