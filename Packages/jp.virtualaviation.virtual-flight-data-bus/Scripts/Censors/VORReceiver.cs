using UdonSharp;
using UnityEngine;
using VirtualCNS;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VORReceiver : FlightDataBusClient
    {
        public int id = 1;

        private NavaidDatabase navaidDatabase;
        private FlightDataFloatValueId frequencyId;
        private FlightDataFloatValueId radialId;
        private FlightDataFloatValueId courseId;
        private FlightDataFloatValueId courceDeviationId;
        private FlightDataBoolValueId tunedId;
        private FlightDataBoolValueId backId;
        private FlightDataBoolValueId localizerId;
        private Transform vorTransform;
        private Vector3 courseRight;
        private float fullCourseDeviationScale;

        private bool _isLocalizer;
        private bool IsLocalizer
        {
            set {
                _isLocalizer = value;
                _WriteAndNotify(localizerId, value);
            }
            get => _isLocalizer;
        }
        private float Radial
        {
            set => _Write(radialId, value);
        }
        private float Course => _Read(courseId);
        private float CourceDeviation
        {
            set => _Write(courceDeviationId, value);
        }

        protected override void OnStart()
        {
            navaidDatabase = NavaidDatabase.GetInstance();

            var offset = Mathf.Max(id - 1, 0);
            frequencyId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Frequency, offset);
            radialId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Radial, offset);
            courseId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Course, offset);
            courceDeviationId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1CourseDeviation, offset);
            tunedId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1Tuned, offset);
            backId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1Back, offset);
            localizerId = FlightDataBus.OffsetValueId(FlightDataBoolValueId.Nav1Localizer, offset);

            _Sbuscribe(frequencyId);
            _Sbuscribe(courseId);
        }

        public override void _OnFloatValueChanged()
        {
            var index = navaidDatabase._FindIndexByFrequency(_Read(frequencyId));

            var tuned = index >= 0;
            if (tuned)
            {
                vorTransform = navaidDatabase.transforms[index];
                IsLocalizer = navaidDatabase._IsILS(index);
                courseRight = IsLocalizer ? vorTransform.forward : Quaternion.AngleAxis(Course, Vector3.up) * vorTransform.right;
                fullCourseDeviationScale = Mathf.Sin((IsLocalizer ? 10.0f : 1.4f) * Mathf.Deg2Rad);
            }
            else
            {
                vorTransform = null;
                Radial = 0;
                CourceDeviation = 0;
            }
            _WriteAndNotify(tunedId, tuned);
        }

        private void Update()
        {
            if (!vorTransform) return;

            var r = transform.position - vorTransform.position;
            Radial = IsLocalizer ? 0 : (Vector3.SignedAngle(Vector3.forward, r, Vector3.up) + 360) % 360;

            var dot = Vector3.Dot(courseRight, r.normalized);
            _Write(backId, dot < 0);
            var rawCoruceDeviation = dot;
            CourceDeviation = Mathf.Clamp(rawCoruceDeviation / fullCourseDeviationScale, -1.0f, 1.0f);
        }
    }
}
