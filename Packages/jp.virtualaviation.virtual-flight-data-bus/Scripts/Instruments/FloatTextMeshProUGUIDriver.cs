using UdonSharp;
using TMPro;

namespace VirtualFlightDataBus
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FloatTextMeshProUGUIDriver : FlightDataBusClient
    {
        public FlightDataFloatValueId valueId;
        public TextMeshProUGUI text;
        public string format = "0.00";

        protected override void OnStart()
        {
            _Sbuscribe(valueId);
        }

        public override void _OnFloatValueChanged()
        {
            text.text = _Read(valueId).ToString(format);
        }
    }
}
