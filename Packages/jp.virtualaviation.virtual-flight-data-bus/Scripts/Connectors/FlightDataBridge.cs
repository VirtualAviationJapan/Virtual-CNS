using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FlightDataBridge : FlightDataBusClient
    {
        public UdonSharpBehaviour sourceBehaviour;
        public FlightDataType dataType;
        public string sourceVariableName;
        public int destinationVariableId;

        private void Update()
        {
            var value = sourceBehaviour.GetProgramVariable(sourceVariableName);

            switch (dataType)
            {
                case FlightDataType.Float:
                    _Write((FlightDataFloatValueId)destinationVariableId, (float)value);
                    break;
                case FlightDataType.Vector3:
                    _Write((FlightDataVector3ValueId)destinationVariableId, (Vector3)value);
                    break;
            }
        }
    }
}
