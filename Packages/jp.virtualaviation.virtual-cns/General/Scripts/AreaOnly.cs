
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MonacaAircrafts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AreaOnly : UdonSharpBehaviour
    {
        public bool initialActive = false;
        public float initializeDelay = 0.5f;
        private void Start()
        {
            if (initialActive) SendCustomEventDelayedSeconds(nameof(_Activate), initializeDelay);
            else SendCustomEventDelayedSeconds(nameof(_Deactivate), initializeDelay);
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player.isLocal) _Activate();
        }
        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (player.isLocal) _Deactivate();
        }

        public void _Activate() => SetActive(true);
        public void _Deactivate() => SetActive(false);

        private void SetActive(bool value)
        {
            for (var i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(value);
        }
    }
}
