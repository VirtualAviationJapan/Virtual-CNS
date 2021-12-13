
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

        private void COM_Start()
        {
            foreach (var transceiver in transceivers)
            {
                transceiver._Subscribe(this);
            }
        }

        public void _Transceiver_Frequency_Changed() => _Dispatch(EVENT_UPDATED);

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
    }
}
