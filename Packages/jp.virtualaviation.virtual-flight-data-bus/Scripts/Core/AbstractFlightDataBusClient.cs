using System;
using UdonSharp;
using UnityEngine;

namespace VirtualFlightDataBus
{
    abstract public class AbstractFlightDataBusClient : AbstractFlightDataBus
    {
        protected FlightDataBus Bus
        {
            private set;
            get;
        }
        private int maxSubscriberCount;

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
            strings = Bus.strings;
            maxSubscriberCount = Bus.maxSubscriberCount;

            subscribers = Bus.subscribers;
            boolSubscriptionMaskList = Bus.boolSubscriptionMaskList;
            floatSubscriptionMaskList = Bus.floatSubscriptionMaskList;
            vector3SubscriptionMaskList = Bus.vector3SubscriptionMaskList;
            stringSubscriptionMaskList = Bus.stringSubscriptionMaskList;

            OnStart();
        }

        protected virtual void OnStart() { }

        private void Subscribe(int id, uint[] maskList, string eventName)
        {
            maskList[SubscriptionIndex] |= FlightDataUtilities.GetMask(id);
            SendCustomEvent(eventName);
        }
        public void _Subscribe(FlightDataBoolValueId id) => Subscribe((int)id, boolSubscriptionMaskList, nameof(_OnBoolValueChanged));
        public void _Subscribe(FlightDataFloatValueId id) => Subscribe((int)id, floatSubscriptionMaskList, nameof(_OnFloatValueChanged));
        public void _Subscribe(FlightDataVector3ValueId id) => Subscribe((int)id, vector3SubscriptionMaskList, nameof(_OnVector3ValueChanged));
        public void _Subscribe(FlightDataStringValueId id) => Subscribe((int)id, stringSubscriptionMaskList, nameof(_OnStringValueChanged));

        private void SendNotify(int id, uint[] maskList, string eventName)
        {
            var mask = FlightDataUtilities.GetMask(id);
            for (var i = 0; i < maxSubscriberCount; i++)
            {
                var subscriber = subscribers[i];
                if (!subscriber) return;
                if ((maskList[i] & mask) != 0) continue;

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
            SendNotify((int)id / 3, vector3SubscriptionMaskList, nameof(_OnVector3ValueChanged));
        }
        public virtual void _OnFloatValueChanged() { }

        public void _Read(FlightDataVector3ValueId id, ref Vector3 value) {
            var i = (int)id;
            value.Set(floats[i], floats[i + 1], floats[i + 2]);
        }
        public void _Write(FlightDataVector3ValueId id, in Vector3 value)
        {
            var i = (int)id;
            floats[i] = value.x;
            floats[i + 1] = value.y;
            floats[i + 2] = value.z;
        }
        public void _WriteAndNotify(FlightDataVector3ValueId id, in Vector3 value)
        {
            _Write(id, value);
            SendNotify((int)id, floatSubscriptionMaskList, nameof(_OnVector3ValueChanged));
        }
        public virtual void _OnVector3ValueChanged() { }

        public string _Read(FlightDataStringValueId id) => strings[(int)id];
        public void _WriteAndNotify(FlightDataStringValueId id, string value)
        {
            strings[(int)id] = value;
            SendNotify((int)id, stringSubscriptionMaskList, nameof(_OnStringValueChanged));
        }
        public virtual void _OnStringValueChanged() { }
    }
}
