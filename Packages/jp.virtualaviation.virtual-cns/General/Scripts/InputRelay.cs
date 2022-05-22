using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InputRelay : UdonSharpBehaviour
    {
        public string value;
        public UdonSharpBehaviour target;
        public string variableName = "input";
        public string eventName = "_OnInput";

        public void _Trigger()
        {
            target.SetProgramVariable(variableName, ((string)target.GetProgramVariable(variableName) + value));
            target.SendCustomEvent(eventName);
        }
    }
}