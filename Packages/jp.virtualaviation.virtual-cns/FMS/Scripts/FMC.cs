
using System;
using UdonRadioCommunication;
using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(1100)] // After Transceiver
    public class FMC : UdonSharpBehaviour
    {
        const int MAX_SUBSCRIBERS = 32;
        const string EVENT_UPDATED = "_FMC_Updated";
        private UdonSharpBehaviour[] subscribers;
        private void Start()
        {
            subscribers = new UdonSharpBehaviour[MAX_SUBSCRIBERS];
            Nav_Start();
        }

        public void _Subscribe(UdonSharpBehaviour subscriber)
        {
            for (var i = 0; i < MAX_SUBSCRIBERS; i++)
            {
                if (!subscribers[i])
                {
                    subscribers[i] = subscriber;
                    return;
                }
            }
        }

        public void _Dispatch(string eventName)
        {
            foreach (var subscriber in subscribers)
            {
                if (!subscriber) return;
                subscriber.SendCustomEvent(eventName);
            }
        }

        [Header("COM")]
        public Transceiver[] transceivers = { };


        public bool _IsValidComFrequency(float value)
        {
            if (value < transceivers[0].minFrequency) return false;
            if (value > transceivers[0].maxFrequency) return false;
            if (value % transceivers[0].frequencyStep != 0) return false;

            return true;
        }
        public float _GetComFrequency(int index)
        {
            return transceivers[index].frequency;
        }
        public void _SetComFrequency(int index, float value)
        {
            transceivers[index]._SetFrequency(value);
            _Dispatch(EVENT_UPDATED);
        }
        public void _IncrementComFrequency(int index) => _SetComFrequency(index, _GetComFrequency(index) + transceivers[index].frequencyStep);
        public void _DecrementComFrequency(int index) => _SetComFrequency(index, _GetComFrequency(index) - transceivers[index].frequencyStep);
        public void _FastIncrementComFrequency(int index) => _SetComFrequency(index, _GetComFrequency(index) + transceivers[index].fastFrequencyStep);
        public void _FastDecrementComFrequency(int index) => _SetComFrequency(index, _GetComFrequency(index) - transceivers[index].fastFrequencyStep);

        public void _IncrementComFrequencyL() => _IncrementComFrequency(0);
        public void _DecrementComFrequencyL() => _DecrementComFrequency(0);
        public void _FastIncrementComFrequencyL() => _FastIncrementComFrequency(0);
        public void _FastDecrementComFrequencyL() => _FastDecrementComFrequency(0);
        public void _IncrementComFrequencyR() => _IncrementComFrequency(1);
        public void _DecrementComFrequencyR() => _DecrementComFrequency(1);
        public void _FastIncrementComFrequencyR() => _FastIncrementComFrequency(1);
        public void _FastDecrementComFrequencyR() => _FastDecrementComFrequency(1);

        [Header("Nav")]
        public NavSelector[] navSelectors = { };

        private NavaidDatabase navaidDatabase;
        private void Nav_Start()
        {
            var ndObj = GameObject.Find(nameof(NavaidDatabase));
            if (ndObj) navaidDatabase = ndObj.GetComponent<NavaidDatabase>();
        }

        public void _SetNavIndex(int index, int value)
        {
            navSelectors[index]._SetIndex(value);
            _Dispatch(EVENT_UPDATED);
        }

        public void _SetNavTargetByIdentity(int index, string identity)
        {
            if (!navaidDatabase) return;

            var navIndex = navaidDatabase._FindIndexByIdentity(identity);
            if (navIndex >= 0) _SetNavIndex(index, navIndex);
        }

        public int _GetNavIndex(int index)
        {
            return navSelectors[index].Index;
        }
        public string _GetNavIdentity(int index)
        {
            return navSelectors[index].Identity;
        }

        public void _NextNav(int index) => _SetNavIndex(index, _GetNavIndex(index) + 1);
        public void _PrevNav(int index) => _SetNavIndex(index, _GetNavIndex(index) - 1);

        public void _NextNavL() => _NextNav(0);
        public void _NextNavR() => _NextNav(1);
    }
}
