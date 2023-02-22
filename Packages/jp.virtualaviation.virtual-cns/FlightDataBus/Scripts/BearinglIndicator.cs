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

        protected override void OnStart()
        {
            radialId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Radial, id - 1);
        }

        private void Update()
        {
            transform.localRotation = Quaternion.AngleAxis(_ReadFloatValue(radialId) + 180, axis);
        }
    }
}
