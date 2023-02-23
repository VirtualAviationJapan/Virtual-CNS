

using UnityEngine;
using VirtualCNS;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(10)]
    public class GlideslopeReceiver : NavigationRadioReceiver
    {
        private readonly float MaxCourseDeviation = Mathf.Sin(8.0f * Mathf.Deg2Rad);
        private FlightDataBoolValueId isILSId;
        private FlightDataBoolValueId capturedId;
        private FlightDataFloatValueId deviationId;

        private bool _captured;
        private bool Captured
        {
            get => _captured;
            set
            {
                if (value == _captured) return;
                _captured = value;
                _WriteAndNotify(capturedId, value);
            }
        }

        private float Deviation
        {
            set
            {
                _Write(deviationId, value);
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            var offset = id - 1;
            isILSId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1ILS, offset);
            capturedId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1GlideslopeCaptured, offset);
            deviationId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1VerticalDeviation, offset);
        }

        private void OnDisable()
        {
            Captured = false;
        }

        protected override void OnTuned()
        {
            if (!NavaidDatabase.IsILS(navaidCapability))
            {
                glideslopeTransform = null;
            }
        }

        private void Update()
        {
            if (!glideslopeTransform)
            {
                enabled = false;
                return;
            }

            var relativePosition = transform.position - glideslopeTransform.position;
            var direction = relativePosition.normalized;
            var distance = relativePosition.magnitude;
            var rawDeviation = NavigationMath.GetGlideslopeRawDeviation(glideslopeTransform.up, direction);

            Captured = NavigationMath.IsGlideslopeAvailable(glideslopeTransform.forward, glideslopeTransform.right, direction, distance, rawDeviation);
            if (Captured) Deviation = NavigationMath.ClampGlideslopeDeviation(rawDeviation);
        }
    }
}
