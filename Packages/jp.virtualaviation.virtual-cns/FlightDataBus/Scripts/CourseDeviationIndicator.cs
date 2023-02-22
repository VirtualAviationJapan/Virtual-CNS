using UnityEngine;
using UdonSharp;

namespace VirtualAviationJapan.FlightDataBus
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CourseDeviationIndicator : FlightDataBusClient
    {
        public int id = 1;
        public IndicatorType type;
        public Vector3 axis = Vector3.right;
        public float maxAngle = 10;

        private FlightDataFloatValueId courseDeviationId;

        protected override void OnStart()
        {
            courseDeviationId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1CourseDeviation, id - 1);
        }

        private void Update()
        {
            var courseDeviation = _ReadFloatValue(courseDeviationId);
            switch (type)
            {
                case IndicatorType.Slide:
                    transform.localPosition = axis * courseDeviation;
                    break;
                case IndicatorType.Rotate:
                    transform.localRotation = Quaternion.AngleAxis(courseDeviation * maxAngle, axis);
                    break;
            }
        }
    }

    public enum IndicatorType
    {
        Slide,
        Rotate,
    }
}
