using UnityEngine;
using UdonSharp;

namespace VirtualFlightDataBus
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CourseDeviationIndicator : AbstractFlightDataBusClient
    {
        public int id = 1;
        public IndicatorType type;
        public Vector3 axis = Vector3.right;
        public float maxAngle = 10;

        private FlightDataFloatValueId courseDeviationId;
        private FlightDataBoolValueId tunedId;

        protected override void OnStart()
        {
            var offset = id - 1;
            courseDeviationId = FlightDataUtilities.OffsetValueId(FlightDataFloatValueId.Nav1CourseDeviation, offset);
            tunedId = FlightDataUtilities.OffsetValueId(FlightDataBoolValueId.Nav1Tuned, offset);
            _Subscribe(tunedId);
        }

        public override void _OnBoolValueChanged()
        {
            var tuned = _Read(tunedId);
            if (gameObject.activeSelf != tuned) gameObject.SetActive(tuned);
        }

        private void Update()
        {
            var courseDeviation = _Read(courseDeviationId);
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
