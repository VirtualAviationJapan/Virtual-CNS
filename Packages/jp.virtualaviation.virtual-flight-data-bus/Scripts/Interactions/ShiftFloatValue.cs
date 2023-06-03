using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ShiftFloatValue : AbstractFlightDataBusClient
    {
        public FlightDataFloatValueId valueId;
        public float step;
        public float fastStep;
        public float minValue = 0;
        public float maxValue = 1;
        public bool loop;
        public bool notify;

        private float Value
        {
            get => _Read(valueId);
            set
            {
                var clamped = Mathf.Clamp(loop ? (value + maxValue) % maxValue : value, minValue, maxValue);
                if (notify) _WriteAndNotify(valueId, clamped);
                else _Write(valueId, clamped);
            }
        }

        public void _Shift(float step)
        {
            Value += step;
        }

        public void _Increment() => _Shift(step);
        public void _FastIncrement() => _Shift(fastStep);
        public void _Decrement() => _Shift(-step);
        public void _FastDecrement() => _Shift(-fastStep);
    }
}
