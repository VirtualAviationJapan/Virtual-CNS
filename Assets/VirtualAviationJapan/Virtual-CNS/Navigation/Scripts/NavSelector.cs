using MonacaAirfrafts;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VirtualAviationJapan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class NavSelector : UdonSharpBehaviour
    {
        public int defaultIndex = 0;
        public float defaultCourse = 0;
        public float courseStep = 10;
        [HideInInspector] public NavaidDatabase database;
        private UdonSharpBehaviour[] subscribers = {};
        [UdonSynced, FieldChangeCallback(nameof(Index))] private int _index;
        public int Index {
            set {
                var count = database.Count;
                if (value >= count) value = 0;
                else if (value < 0) value = count - 1;

                if (value == _index) return;
                _index = value;

                foreach (var subscriber in subscribers)
                {
                    if (subscriber != null) subscriber.SendCustomEvent("_NavChanged");
                }
            }
            get => _index;
        }
        [UdonSynced, FieldChangeCallback(nameof(Course))] private float _course;
        public float Course {
            set {
                value = (value % 360 + 360) % 360;
                if (_course == value) return;
                _course = value;

                foreach (var subscriber in subscribers)
                {
                    if (subscriber != null) subscriber.SendCustomEvent("_NavChanged");
                }
            }
            get => _course;
        }

        public string Identity => database.identities[Index];
        public Transform NavaidTransform => database.transforms[Index];
        public Transform GlideSlopeTransform => database.glideSlopeTransforms[Index];
        public Transform DMETransform => database.dmeTransforms[Index];
        public bool IsILS => database._IsILS(Index);
        public bool IsVOR => database._IsVOR(Index);
        public bool HasDME => database._HasDME(Index);

        private void Start()
        {
            subscribers = new UdonSharpBehaviour[32];
            database = (NavaidDatabase)GameObject.Find(nameof(NavaidDatabase)).GetComponent(typeof(UdonBehaviour));

            Index = defaultIndex;
            Course = defaultCourse;

            Debug.Log($"[Virtual-CNS][{this}:{GetHashCode():X8}] Iniialized", gameObject);
        }

        public void _Subscribe(UdonSharpBehaviour subscriber)
        {
            for (var i = 0; i < subscribers.Length; i++)
            {
                if (subscribers[i] == null)
                {
                    subscribers[i] = subscriber;
                    subscriber.SendCustomEvent("_NavChanged");
                    return;
                }
                if (subscribers[i] == subscriber) return;
            }
        }

        public void _TakeOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public void _SetIndex(int value)
        {
            _TakeOwnership();
            Index = value;
            RequestSerialization();
        }

        public void _IncrementIndex()
        {
            _TakeOwnership();
            Index++;
            RequestSerialization();
        }

        public void _DecrementIndex()
        {
            _TakeOwnership();
            Index--;
            RequestSerialization();
        }

        public void _StepCourse()
        {
            _TakeOwnership();
            Course += courseStep;
            RequestSerialization();
        }
        public void _IncrementCourse()
        {
            _TakeOwnership();
            Course++;
            RequestSerialization();
        }
        public void _DecrementCourse()
        {
            _TakeOwnership();
            Course--;
            RequestSerialization();
        }
        public void _FastIncrementCourse()
        {
            _TakeOwnership();
            Course += courseStep;
            RequestSerialization();
        }
        public void _FastDecrementCourse()
        {
            _TakeOwnership();
            Course -= courseStep;
            RequestSerialization();
        }
    }
}
