using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
{

    [DefaultExecutionOrder(10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PitotTube : FlightDataBusClient
    {
        public float minIAS = 20.0f / FlightDataBus.Knots;
        private Vector3 previousPosition;

        private void OnEnable()
        {
            previousPosition = transform.position;
        }

        private void Update()
        {
            var position = transform.position;
            var deltaTime = Time.deltaTime;

            _Write(FlightDataFloatValueId.TAS, Mathf.Max(Vector3.Dot((position - previousPosition) * deltaTime - _Read(FlightDataVector3ValueId.Wind), transform.forward), minIAS));
            _Write(FlightDataFloatValueId.Altitude, position.y - _Read(FlightDataFloatValueId.SeaLevel));
            _Write(FlightDataFloatValueId.VerticalSpeed, (position.y - previousPosition.y) * deltaTime);

            previousPosition = position;
        }
    }
}
