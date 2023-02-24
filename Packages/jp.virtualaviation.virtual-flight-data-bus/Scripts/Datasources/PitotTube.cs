using System.Runtime.InteropServices;
using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{

    [DefaultExecutionOrder(10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PitotTube : AbstractFlightDataBusClient
    {
        public float minIAS = 20.0f / FlightDataUtilities.Knots;
        private Vector3 previousPosition;
        private Vector3 wind;

        private void OnEnable()
        {
            previousPosition = transform.position;
        }

        private void Update()
        {
            var position = transform.position;
            var deltaTime = Time.deltaTime;

            _Read(FlightDataVector3ValueId.Wind, ref wind);

            _Write(FlightDataFloatValueId.TAS, Mathf.Max(Vector3.Dot((position - previousPosition) / deltaTime - wind, transform.forward), minIAS));
            _Write(FlightDataFloatValueId.Altitude, position.y - _Read(FlightDataFloatValueId.SeaLevel));
            _Write(FlightDataFloatValueId.VerticalSpeed, (position.y - previousPosition.y) / deltaTime);

            previousPosition = position;
        }
    }
}
