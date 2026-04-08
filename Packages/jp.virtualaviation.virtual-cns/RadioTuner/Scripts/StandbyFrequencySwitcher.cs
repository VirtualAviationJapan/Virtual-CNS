using UdonSharp;
using UnityEngine;

namespace VirtualCNS
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

        public void _RequestSerialization()
        {
            active.RequestSerialization();
            standby.RequestSerialization();
        }

        [System.Obsolete("Use _RequestSerialization() instead.")]
        public void _RequenstSerialization() => _RequestSerialization();

        public void _TransferFrequency()
        {
            _TakeOwnership();

            var tmp = standby.Frequency;
            standby.Frequency = active.Frequency;
            active.Frequency = tmp;

            _RequestSerialization();
        }
    }
}
