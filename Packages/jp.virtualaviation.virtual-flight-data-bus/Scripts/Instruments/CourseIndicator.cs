using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CourseIndicator : AbstractFlightDataBusClient
    {
        public FlightDataNavId id;
        public Vector3 axis = Vector3.back;
        private FlightDataFloatValueId courseId;

        protected override void OnStart()
        {
            courseId = FlightDataUtilities.OffsetValueId(FlightDataFloatValueId.Nav1Course, (int)id);
            _Subscribe(courseId);
        }

        public override void _OnFloatValueChanged()
        {
            transform.localRotation = Quaternion.AngleAxis(_Read(courseId), axis);
        }
    }
}
