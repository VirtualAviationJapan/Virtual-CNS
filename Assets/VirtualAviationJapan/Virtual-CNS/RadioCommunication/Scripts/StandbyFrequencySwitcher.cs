using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class StandbyFrequencySwitcher : UdonSharpBehaviour
    {
        public RadioTuner active, standby;

        public void _TakeOwnership()
        {
            active._TakeOwnership();
            standby._TakeOwnership();
        }

        public void _RequenstSerialization()
        {
            active.RequestSerialization();
            standby.RequestSerialization();
        }

        public void _TransferFrequency()
        {
            _TakeOwnership();

            var tmp = standby.Frequency;
            standby.Frequency = active.Frequency;
            active.Frequency = tmp;

            _RequenstSerialization();
        }
    }
}
