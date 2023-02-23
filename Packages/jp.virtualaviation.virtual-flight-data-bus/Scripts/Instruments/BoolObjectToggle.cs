using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BoolObjectToggle : FlightDataBusClient
    {
        public FlightDataBoolValueId valueId;
        public bool invert = false;

        protected override void OnStart()
        {
            _Subscribe(valueId);
        }

        public override void _OnBoolValueChanged()
        {
            var value = _Read(valueId) ^ invert;
            if (value != gameObject.activeSelf) gameObject.SetActive(value);
        }
    }
}
