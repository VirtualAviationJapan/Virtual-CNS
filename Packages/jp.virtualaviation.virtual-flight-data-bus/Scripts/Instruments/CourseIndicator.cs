using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    public class CourseIndicator : FlightDataBusClient
    {
        public int id = 1;
        public Vector3 axis = Vector3.back;
        private FlightDataFloatValueId courseId;

        protected override void OnStart()
        {
            courseId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Course, id - 1);
            _Sbuscribe(courseId);
        }

        public override void _OnFloatValueChanged()
        {
            transform.localRotation = Quaternion.AngleAxis(_Read(courseId), axis);
        }
    }
}
