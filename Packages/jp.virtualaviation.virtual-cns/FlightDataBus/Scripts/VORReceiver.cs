using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Signers;
using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
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
        private Transform vorTransform;
        private Vector3 courceDirection;
        private float fullCourseDeviationScale;

        private float Radial
        {
            set => _WriteFloatValue(radialId, value);
        }
        private float Course => _ReadFloatValue(courseId);
        private float CourceDeviation
        {
            set => _WriteFloatValue(courceDeviationId, value);
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

            _SubscribeFloatValue(frequencyId);
            _SubscribeFloatValue(courseId);
        }

        public override void _OnFloatValueChanged()
        {
            var index = navaidDatabase._FindIndexByFrequency(_ReadFloatValue(frequencyId));

            var tuned = index < 0;
            if (tuned)
            {
                vorTransform = navaidDatabase.transforms[index];
                var isLocalizer = navaidDatabase._IsILS(index);
                courceDirection = isLocalizer ? vorTransform.forward : Quaternion.AngleAxis(Course, Vector3.up) * vorTransform.forward;
                fullCourseDeviationScale = isLocalizer ? 10.0f : 1.4f;
            }
            else
            {
                vorTransform = null;
                Radial = 0;
                CourceDeviation = 0;
            }
            _WriteBoolValueAndNotify(tunedId, tuned);
        }

        private void Update()
        {
            if (!vorTransform) return;

            var r = transform.position - vorTransform.position;
            Radial = (Vector3.SignedAngle(Vector3.forward, r, Vector3.up) + 360) % 360;

            var dot = Vector3.Dot(courceDirection, r.normalized);
            _WriteBoolValue(backId, dot < 0);
            var rawCoruceDeviation = Mathf.Asin(dot) * Mathf.Rad2Deg;
            CourceDeviation = Mathf.Clamp(rawCoruceDeviation / fullCourseDeviationScale, -1.0f, 1.0f);
        }
    }
}
