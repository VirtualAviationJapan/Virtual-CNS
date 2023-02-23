using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{

    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BearinglIndicator : FlightDataBusClient
    {
        public int id = 1;
        public Vector3 axis = Vector3.back;
        private FlightDataFloatValueId bearingId;
        private FlightDataBoolValueId tunedId;
        private FlightDataBoolValueId localizerId;

        protected override void OnStart()
        {
            var offset = id - 1;
            bearingId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Bearing, offset);
            tunedId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1Tuned, offset);
            localizerId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1ILS, offset);
            _Subscribe(tunedId);
            _Subscribe(localizerId);
        }

        public override void _OnBoolValueChanged()
        {
            var hasBearing = _Read(tunedId) && !_Read(localizerId);
            if (gameObject.activeSelf != hasBearing) gameObject.SetActive(hasBearing);
        }

        private void Update()
        {
            transform.localRotation = Quaternion.AngleAxis(_Read(bearingId), axis);
        }
    }
}
