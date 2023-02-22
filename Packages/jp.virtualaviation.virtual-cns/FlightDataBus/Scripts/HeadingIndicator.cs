using System;
using UdonSharp;
using UnityEngine;


namespace VirtualAviationJapan.FlightDataBus
{
    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class HeadingIndicator : FlightDataBusClient
    {
        public HeadingIndicatorType type;
        public Vector3 axis = Vector3.forward;

        private void Update()
        {
            transform.localRotation = Quaternion.AngleAxis(_ReadFloatValue(type == HeadingIndicatorType.Gyro ? FlightDataFloatValueId.Heading : FlightDataFloatValueId.MagneticHeading), axis);
        }
    }

    public enum HeadingIndicatorType
    {
        Gyro,
        Magnetic,
    }
}
