using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
{

    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BearinglIndicator : FlightDataBusClient
    {
        public int id = 1;
        public Vector3 axis = Vector3.back;
        private FlightDataFloatValueId radialId;
        private FlightDataBoolValueId tunedId;
        private FlightDataBoolValueId localizerId;

        protected override void OnStart()
        {
            var offset = id - 1;
            radialId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Radial, offset);
            tunedId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1Tuned, offset);
            localizerId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1Localizer, offset);
            _Sbuscribe(tunedId);
            _Sbuscribe(localizerId);
        }

        public override void _OnBoolValueChanged()
        {
            var hasRadial = _Read(tunedId) && !_Read(localizerId);
            if (gameObject.activeSelf != hasRadial) gameObject.SetActive(hasRadial);
        }

        private void Update()
        {
            transform.localRotation = Quaternion.AngleAxis(_Read(radialId) + 180, axis);
        }
    }
}
