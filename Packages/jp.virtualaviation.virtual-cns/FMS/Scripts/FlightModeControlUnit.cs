using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FlightModeControlUnit : UdonSharpBehaviour
    {
        public const ushort MODE_AUTO_PILOT = 0x01;
        public const ushort MODE_ALTITUDE_HOLD = 0x02;
        public const ushort MODE_LNAV = 0x08;
        public const ushort MODE_VERTICAL_SPEED = 0x10;
        public const ushort MODE_VNAV = 0x20;
        public const ushort MODE_APPROACH = 0x40;
        public const ushort MODE_SPEED = 0x80;
        public const ushort MODE_AUTO_THROTTLE = 0x100;

        public float magneticDeclination = 0.0f;

        [UdonSynced][FieldChangeCallback(nameof(Heading))] private float _heading;
        public float Heading { get => _heading; set => _heading = value; }

        [UdonSynced][FieldChangeCallback(nameof(Airspeed))] private float _airspeed;
        public float Airspeed { get => _airspeed; set => _airspeed = value; }

        [UdonSynced][FieldChangeCallback(nameof(Altitude))] private float _altitude;
        public float Altitude { get => _altitude; set => _altitude = value; }

        [UdonSynced][FieldChangeCallback(nameof(VerticalSpeed))] private float _verticalSpeed;
        public float VerticalSpeed { get => _verticalSpeed; set => _verticalSpeed = value; }

        [UdonSynced][FieldChangeCallback(nameof(Mode))] private byte _mode;
        public byte Mode { get => _mode; set => _mode = value; }

        private Transform origin;

        private void Start()
        {
            var navaidDatabaseObj = GameObject.Find("NavaidDatabase");
            if (navaidDatabaseObj) magneticDeclination = (float)((UdonBehaviour)navaidDatabaseObj.GetComponent(typeof(UdonBehaviour))).GetProgramVariable("magneticDeclination");

            var rigidbody = GetComponentInParent<Rigidbody>();
            origin = rigidbody ? rigidbody.transform : transform;
        }

        public void _TakeOwnership()
        {
            // if (Networking.IsOwner(gameObject)) return;
            // Networking.SetOwner(gameObject);
        }
    }
}
