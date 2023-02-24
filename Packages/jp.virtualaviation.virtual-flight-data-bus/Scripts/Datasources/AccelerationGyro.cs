using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AccelerationGyro : AbstractFlightDataBusClient
    {
        private Vector3 previousPosition, previousVelocity;

        private void OnEnable()
        {
            previousPosition = transform.position;
            previousVelocity = Vector3.zero;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            var position = transform.position;
            var velocity = (position - previousPosition) / deltaTime;
            var gravity = Physics.gravity;
            var load = ((velocity - previousVelocity) / deltaTime + gravity) / gravity.magnitude;

            _Write(FlightDataFloatValueId.LateralLoad, load.x);
            _Write(FlightDataFloatValueId.VerticalLoad, load.y);
            _Write(FlightDataFloatValueId.LongitudinalLoad, load.z);

            previousPosition = position;
            previousVelocity = velocity;
        }
    }
}
