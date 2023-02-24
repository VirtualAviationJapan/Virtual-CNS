using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BearinglIndicator : AbstractFlightDataBusClient
    {
        public FlightDataNavId id;
        public Vector3 axis = Vector3.back;
        private FlightDataFloatValueId bearingId;
        private FlightDataBoolValueId tunedId;
        private FlightDataBoolValueId ilsId;

        protected override void OnStart()
        {
            bearingId = FlightDataUtilities.OffsetValueId(FlightDataFloatValueId.Nav1Bearing, (int)id);
            tunedId = FlightDataUtilities.OffsetValueId(FlightDataBoolValueId.Nav1Tuned, (int)id);
            ilsId = FlightDataUtilities.OffsetValueId(FlightDataBoolValueId.Nav1ILS, (int)id);
            _Subscribe(tunedId);
            _Subscribe(ilsId);
        }

        public override void _OnBoolValueChanged()
        {
            var hasBearing = _Read(tunedId) && !_Read(ilsId);
            if (gameObject.activeSelf != hasBearing) gameObject.SetActive(hasBearing);
        }

        private void Update()
        {
            transform.localRotation = Quaternion.AngleAxis(_Read(bearingId), axis);
        }
    }
}
