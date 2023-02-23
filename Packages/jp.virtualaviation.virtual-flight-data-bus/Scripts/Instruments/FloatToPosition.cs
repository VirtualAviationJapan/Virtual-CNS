using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FloatToPosition : FlightDataBusClient
    {
        public FlightDataFloatValueId valueId;
        public Vector3 axis = Vector3.right;
        public bool update;

        protected override void OnStart()
        {
            if (!update) _Subscribe(valueId);
            enabled = update;
        }

        public override void _OnFloatValueChanged()
        {
            transform.localPosition = _Read(valueId) * axis;
        }

        private void Update() => _OnFloatValueChanged();
    }
}
