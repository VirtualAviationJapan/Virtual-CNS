using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VirtualAviationJapan
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(1200)] // After FMC, Before CDU
    public class CDUFunction : UdonSharpBehaviour
    {
        public void _OnPageEnter(CDU cdu) {}
        public void _Refresh() {}
        public void _L1() {}
        public void _L2() {}
        public void _L3() {}
        public void _L4() {}
        public void _L5() {}
        public void _L6() {}
        public void _R1() {}
        public void _R2() {}
        public void _R3() {}
        public void _R4() {}
        public void _R5() {}
        public void _R6() {}
    }
}
