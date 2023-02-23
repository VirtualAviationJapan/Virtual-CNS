using UdonSharp;
using UnityEngine;
using VirtualCNS;

namespace VirtualFlightDataBus
{
    public class NavigationRadioReceiver : FlightDataBusClient
    {
        public int id = 1;

        private NavaidDatabase navaidDatabase;
        private FlightDataFloatValueId frequencyId;
        protected float magneticDeclination;
        protected int navaidIndex;
        protected Transform navaidTransform;
        protected string navaidIdentity;
        protected uint navaidCapability;
        protected Transform glideslopeTransform;

        public virtual uint _RequiredCapability => 0;

        protected override void OnStart()
        {
            navaidDatabase = NavaidDatabase.GetInstance();
            magneticDeclination = navaidDatabase.magneticDeclination;

            var offset = Mathf.Max(id - 1, 0);
            frequencyId = FlightDataBus.OffsetValueId(FlightDataFloatValueId.Nav1Frequency, offset);

            _Subscribe(frequencyId);
        }

        public override void _OnFloatValueChanged()
        {
            navaidIndex = navaidDatabase._FindIndexByFrequency(_Read(frequencyId));

            var requiredCapablity = _RequiredCapability;
            var tuned = navaidIndex >= 0 && (requiredCapablity == 0 || (requiredCapablity & navaidDatabase.capabilities[navaidIndex]) != 0);
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
