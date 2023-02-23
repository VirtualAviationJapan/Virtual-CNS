using UdonSharp;
using TMPro;
using UnityEngine;

namespace VirtualFlightDataBus
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [DefaultExecutionOrder(100)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FloatToText : FlightDataBusClient
    {
        public FlightDataFloatValueId valueId;
        public string format = "0.00";
        public float offset;
        public float multiplier = 1.0f;
        public bool update;

        private TextMeshProUGUI text;


        protected override void OnStart()
        {
            text = GetComponent<TextMeshProUGUI>();
            if (!update) _Subscribe(valueId);
            enabled = update;
        }

        public override void _OnFloatValueChanged()
        {
            text.text = (_Read(valueId) * multiplier + offset).ToString(format);
        }

        private void Update() => _OnFloatValueChanged();
    }
}
