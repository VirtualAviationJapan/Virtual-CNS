using System;
using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    public class FlightDataBusClient : UdonSharpBehaviour
    {
        private static uint GetMask(int n)
        {
            return 1u << n;
        }

        protected FlightDataBus Bus
        {
            private set;
            get;
        }
        private int maxSubscriberCount;

        private UdonSharpBehaviour[] subscribers;
        private uint[] boolSubscriptionMaskList;
        private uint[] floatSubscriptionMaskList;
        private uint[] vector3SubscriptionMaskList;
        private bool[] bools;
        private float[] floats;
        private Vector3[] vector3s;


        private int _subscriptionIndex = -1;
        private int SubscriptionIndex
        {
            get
            {

                if (_subscriptionIndex >= 0) return _subscriptionIndex;

                var i = Array.IndexOf(subscribers, null);
                if (i >= 0) subscribers[i] = this;
                _subscriptionIndex = i;

                return i;
            }
        }

        private void Start()
        {
            Bus = GetComponentInParent<FlightDataBus>();

            bools = Bus.bools;
            floats = Bus.floats;
            vector3s = Bus.vector3s;
            maxSubscriberCount = Bus.maxSubscriberCount;

            subscribers = Bus.subscribers;
            boolSubscriptionMaskList = Bus.boolSubscriptionMaskList;
            floatSubscriptionMaskList = Bus.floatSubscriptionMaskList;
            vector3SubscriptionMaskList = Bus.vector3SubscriptionMaskList;

            OnStart();
        }

        protected virtual void OnStart() { }

        public void _Sbuscribe(FlightDataBoolValueId id)
        {
            boolSubscriptionMaskList[SubscriptionIndex] |= GetMask((int)id);
            _OnBoolValueChanged();
        }
        public void _Sbuscribe(FlightDataFloatValueId id)
        {
            floatSubscriptionMaskList[SubscriptionIndex] |= GetMask((int)id);
            _OnFloatValueChanged();
        }
        public void _Sbuscribe(FlightDataVector3ValueId id)
        {
            vector3SubscriptionMaskList[SubscriptionIndex] |= GetMask((int)id);
            _OnVector3ValueChanged();
        }

        private void SendNotify(int id, uint[] maskList, string eventName)
        {
            var mask = GetMask(id);
            for (var i = 0; i < maxSubscriberCount; i++)
            {
                var subscriber = subscribers[i];
                if (!subscriber) return;
                if ((maskList[i] & mask) == 0) continue;

                subscriber.SendCustomEvent(eventName);
            }
        }

        public bool _Read(FlightDataBoolValueId id) => bools[(int)id];
        public void _Write(FlightDataBoolValueId id, bool value) => bools[(int)id] = value;
        public void _WriteAndNotify(FlightDataBoolValueId id, bool value)
        {
            _Write(id, value);
            SendNotify((int)id, boolSubscriptionMaskList, nameof(_OnBoolValueChanged));
        }
        public virtual void _OnBoolValueChanged() { }

        public float _Read(FlightDataFloatValueId id) => floats[(int)id];
        public void _Write(FlightDataFloatValueId id, float value) => floats[(int)id] = value;
        public void _WriteAndNotify(FlightDataFloatValueId id, float value)
        {
            _Write(id, value);
            SendNotify((int)id, floatSubscriptionMaskList, nameof(_OnFloatValueChanged));
        }
        public virtual void _OnFloatValueChanged() { }

        public Vector3 _Read(FlightDataVector3ValueId id) => vector3s[(int)id];
        public void _Write(FlightDataVector3ValueId id, Vector3 value) => vector3s[(int)id] = value;
        public void _WriteAndNotify(FlightDataVector3ValueId id, Vector3 value)
        {
            _Write(id, value);
            SendNotify((int)id, floatSubscriptionMaskList, nameof(_OnVector3ValueChanged));
        }
        public virtual void _OnVector3ValueChanged() { }
    }
}
