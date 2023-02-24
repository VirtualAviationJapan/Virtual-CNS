using PlasticGui;
using UdonSharp;
using UnityEngine;
using VirtualCNS;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VORReceiver : NavigationRadioReceiver
    {
        private FlightDataFloatValueId bearingId;
        private FlightDataFloatValueId courseId;
        private FlightDataFloatValueId courceDeviationId;
        private FlightDataBoolValueId tunedId;
        private FlightDataBoolValueId backId;
        private FlightDataBoolValueId isILSId;

        private bool IsILS
        {
            get => _Read(isILSId);
            set
            {
                _WriteAndNotify(isILSId, value);
            }
        }

        private bool _tuned;
        private bool Tuned
        {
            get => _tuned;
            set
            {
                if (value == _tuned) return;
                _tuned = value;
                _WriteAndNotify(tunedId, value);
            }
        }

        private float Course => _Read(courseId);

        private float Bearing
        {
            get => _Read(bearingId);
            set
            {
                _Write(bearingId, value);
            }
        }

        private bool _back;
        private bool Back
        {
            get => _back;
            set
            {
                _back = value;
                _Write(backId, value);
            }
        }
        private float CourseDeviation
        {
            set => _Write(courceDeviationId, value);
        }

        protected override void OnStart()
        {
            base.OnStart();

            var offset = (int)id;

            bearingId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Bearing, offset);
            courseId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Course, offset);
            courceDeviationId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1CourseDeviation, offset);
            tunedId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1Tuned, offset);
            backId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1Back, offset);
            isILSId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1ILS, offset);
        }

        protected override bool CheckCapability(uint capability)
        {
            return NavaidDatabase.IsVOR(capability) || NavaidDatabase.IsILS(capability);
        }

        protected override void OnTuned()
        {
            IsILS = NavaidDatabase.IsILS(navaidCapability);
            Back = false;
        }

        private void OnDisable()
        {
            Tuned = false;
        }

        private void Update()
        {
            if (!navaidTransform)
            {
                enabled = false;
                return;
            }

            var relativePosition = transform.position - navaidTransform.position;
            var direction = relativePosition.normalized;
            var distance = relativePosition.magnitude;

            if (IsILS)
            {
                var isBehind = NavigationMath.IsBehind(navaidTransform.forward, relativePosition);
                var rawDeviation = NavigationMath.GetLocalizerRawDeviation(navaidTransform.right, direction);
                Tuned = NavigationMath.IsLocalizerAvailable(rawDeviation, distance, isBehind);
                if (Tuned)
                {
                    CourseDeviation = NavigationMath.ClampLocalizerDeviation(rawDeviation);
                }
            }
            else
            {
                Tuned = NavigationMath.IsVORAvailable(direction, distance);
                if (Tuned)
                {
                    Bearing = NavigationMath.GetBearing(relativePosition) - magneticDeclination;
                    Back = NavigationMath.IsBehind(Bearing, Course);
                    CourseDeviation = NavigationMath.GetVORDeviation(Bearing, Course, Back);
                }
            }
        }
    }
}
