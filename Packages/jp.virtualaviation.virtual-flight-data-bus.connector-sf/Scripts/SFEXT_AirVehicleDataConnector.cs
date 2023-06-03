using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus.Connectors.SaccFlight
{
    [DefaultExecutionOrder(0)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class SFEXT_AirVehicleDataConnector : AbstractFlightDataBusClient
    {
        private SaccAirVehicle airVehicle;
        private bool isPilot;

        [UdonSynced][FieldChangeCallback(nameof(Wind))] private Vector3 _wind;
        private Vector3 Wind
        {
            get => _wind;
            set
            {
                _wind = value;
                _Write(FlightDataVector3ValueId.Wind, value);
            }
        }

        public void SFEXT_EntityStart()
        {
            var rigidbody = GetComponentInParent<Rigidbody>();
            var entity = rigidbody.GetComponentInParent<SaccEntity>();
            airVehicle = (SaccAirVehicle)entity.GetExtention(GetUdonTypeName<SaccAirVehicle>());

            _WriteAndNotify(FlightDataFloatValueId.SeaLevel, airVehicle.SeaLevel);
        }

        public void SFEXT_L_PilotEnter()
        {
            isPilot = true;
        }
        public void SFEXT_L_PilotExit()
        {
            isPilot = false;
        }

        public void SFEXT_L_KeepAwake()
        {
            enabled = true;
        }

        public void SFEXT_L_KeepAwakeFalse()
        {
            enabled = false;
        }

        public void Update()
        {
            if (!isPilot) return;
            Wind = airVehicle.Wind;
        }
    }
}
