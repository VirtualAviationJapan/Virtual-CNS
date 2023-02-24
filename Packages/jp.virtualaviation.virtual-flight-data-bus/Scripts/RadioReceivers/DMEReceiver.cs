using UdonSharp;
using UnityEngine;
using VirtualCNS;

namespace VirtualFlightDataBus
{
    [DefaultExecutionOrder(10)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DMEReceiver : NavigationRadioReceiver
    {
        private FlightDataFloatValueId dmeId;

        protected override void OnStart()
        {
            base.OnStart();

            dmeId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Distance, (int)id);
        }

        protected override bool CheckCapability(uint capability)
        {
            return NavaidDatabase.HasDME(capability);
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
