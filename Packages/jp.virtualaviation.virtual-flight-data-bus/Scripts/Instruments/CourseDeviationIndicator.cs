using UnityEngine;
using UdonSharp;

namespace VirtualFlightDataBus
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CourseDeviationIndicator : AbstractFlightDataBusClient
    {
        public FlightDataNavId id;
        public IndicatorType type;
        public Vector3 axis = Vector3.right;
        public float maxAngle = 10;

        private FlightDataFloatValueId courseDeviationId;
        private FlightDataBoolValueId tunedId;

        protected override void OnStart()
        {
            courseDeviationId = FlightDataUtilities.OffsetValueId(FlightDataFloatValueId.Nav1CourseDeviation, (int)id);
            tunedId = FlightDataUtilities.OffsetValueId(FlightDataBoolValueId.Nav1Tuned, (int)id);
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
