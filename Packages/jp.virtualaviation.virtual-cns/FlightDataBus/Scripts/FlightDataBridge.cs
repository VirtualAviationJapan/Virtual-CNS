using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlightDataBridge : FlightDataBusClient
    {
        public UdonSharpBehaviour sourceBehaviour;
        public FlightDataType dataType;
        public string sourceVariableName;
        public int destinationVariableId;

        public void Update()
        {
            var value = sourceBehaviour.GetProgramVariable(sourceVariableName);

            switch (dataType)
            {
                case FlightDataType.Float:
                    _WriteFloatValue((FlightDataFloatValueId)destinationVariableId, (float)value);
                    break;
                case FlightDataType.Vector3:
                    _WriteVector3Value((FlightDataVector3ValueId)destinationVariableId, (Vector3)value);
                    break;
            }
        }
    }
}
