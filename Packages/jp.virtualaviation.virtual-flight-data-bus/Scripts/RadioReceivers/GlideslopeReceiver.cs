

using UdonSharp;
using UnityEngine;
using VirtualCNS;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GlideslopeReceiver : NavigationRadioReceiver
    {
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

            capturedId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1GlideslopeCaptured, (int)id);
            deviationId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1VerticalDeviation, (int)id);
        }

        private void OnDisable()
        {
            Captured = false;
        }

        protected override bool CheckCapability(uint capability)
        {
            return NavaidDatabase.IsILS(capability);
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
