using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
{
    public class CourseIndicator : FlightDataBusClient
    {
        public int id = 1;
        public Vector3 axis = Vector3.forward;
        private FlightDataFloatValueId courseId;

        protected override void OnStart()
        {
            courseId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Course, id - 1);
            _SubscribeFloatValue(courseId);
        }

        public override void _OnFloatValueChanged()
        {
            transform.localRotation = Quaternion.AngleAxis(_ReadFloatValue(courseId), axis);
        }
    }
}
