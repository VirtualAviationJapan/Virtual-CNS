using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AttitudeIndicator : FlightDataBusClient
    {
        public Transform rollIndicator;
        public Vector3 rollIndicatorAxis = Vector3.forward;
        public float maxRoll = 180;
        public Transform pitchIndicator;
        public Vector3 pitchIndicatorAxis = Vector3.up;
        public float maxPitch = 90;

        private void Update()
        {
            rollIndicator.localRotation = Quaternion.AngleAxis(Mathf.Clamp(-_Read(FlightDataFloatValueId.Roll), -maxRoll, maxRoll), rollIndicatorAxis);
            pitchIndicator.localPosition = Mathf.Clamp(-_Read(FlightDataFloatValueId.Pitch)  / maxPitch, -1, 1) * pitchIndicatorAxis;
        }
    }
}
