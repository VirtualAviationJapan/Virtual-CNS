using UdonSharp;
using UnityEngine;
using VirtualCNS;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DMEReceiver : NavigationRadioReceiver
    {
        public override uint _RequiredCapability => (uint)NavaidCapability.DME;

        private FlightDataFloatValueId dmeId;


        protected override void OnStart()
        {
            base.OnStart();

            dmeId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Distance, id - 1);
        }


        private void Update()
        {
            if (!navaidTransform)
            {
                enabled = false;
                return;
            }

            _Write(dmeId, Vector3.Distance(navaidTransform.position, transform.position));
        }
    }
}
