using UdonSharp;
using UnityEngine;
using VirtualCNS;

namespace VirtualFlightDataBus
{
    public class NavigationRadioReceiver : AbstractFlightDataBusClient
    {
        public FlightDataNavId id;

        private NavaidDatabase navaidDatabase;
        private FlightDataFloatValueId frequencyId;
        protected float magneticDeclination;
        protected int navaidIndex;
        protected Transform navaidTransform;
        protected string navaidIdentity;
        protected uint navaidCapability;
        protected Transform glideslopeTransform;

        protected override void OnStart()
        {
            navaidDatabase = NavaidDatabase.GetInstance();
            magneticDeclination = navaidDatabase.magneticDeclination;

            frequencyId = FlightDataUtilities.OffsetValueId(FlightDataFloatValueId.Nav1Frequency, (int)id);

            _Subscribe(frequencyId);
        }


        protected virtual bool CheckCapability(uint capability) => true;
        public override void _OnFloatValueChanged()
        {
            navaidIndex = navaidDatabase._FindIndexByFrequency(_Read(frequencyId));

            var tuned = navaidIndex >= 0 && CheckCapability(navaidDatabase.capabilities[navaidIndex]);
            if (tuned)
            {
                navaidTransform = navaidDatabase.transforms[navaidIndex];
                navaidIdentity = navaidDatabase.identities[navaidIndex];
                navaidCapability = navaidDatabase.capabilities[navaidIndex];
                glideslopeTransform = navaidDatabase.glideSlopeTransforms[navaidIndex];
                OnTuned();
            }
            else
            {
                navaidTransform = null;
                navaidIdentity = null;
                navaidCapability = 0;
                glideslopeTransform = null;
            }

            enabled = tuned;
        }

        protected virtual void OnTuned() { }
    }
}
