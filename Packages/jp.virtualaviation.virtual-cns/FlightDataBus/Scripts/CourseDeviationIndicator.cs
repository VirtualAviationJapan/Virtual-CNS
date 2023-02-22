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
        private FlightDataBoolValueId tunedId;

        protected override void OnStart()
        {
            var offset = id - 1;
            courseDeviationId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1CourseDeviation, offset);
            tunedId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1Tuned, offset);
            _Sbuscribe(tunedId);
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
