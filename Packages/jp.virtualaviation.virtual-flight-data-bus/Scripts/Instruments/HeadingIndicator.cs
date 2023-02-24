using System;
using UdonSharp;
using UnityEngine;


namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class HeadingIndicator : AbstractFlightDataBusClient
    {
        public HeadingIndicatorType type;
        public Vector3 axis = Vector3.forward;

        private void Update()
        {
            transform.localRotation = Quaternion.AngleAxis(_Read(type == HeadingIndicatorType.Gyro ? FlightDataFloatValueId.Heading : FlightDataFloatValueId.MagneticHeading), axis);
        }
    }

    public enum HeadingIndicatorType
    {
        Gyro,
        Magnetic,
    }
}
