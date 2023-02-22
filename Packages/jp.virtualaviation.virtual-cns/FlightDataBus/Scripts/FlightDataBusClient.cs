using System;
using UdonSharp;
using UnityEngine;

namespace VirtualAviationJapan.FlightDataBus
{
    public class FlightDataBusClient : UdonSharpBehaviour
    {
        private static uint GetMask(int n)
        {
            return 1u >> n;
        }

        protected FlightDataBus Bus
        {
            private set;
            get;
        }
        private int maxSubscriberCount;
        private int subscriptionIndex = -1;
        private UdonSharpBehaviour[] subscribers;
        private uint[] boolSubscriptionMaskList;
        private uint[] floatSubscriptionMaskList;
        private uint[] vector3SubscriptionMaskList;
        private bool[] bools;
        private float[] floats;
        private Vector3[] vector3s;

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

        private int GetSubscriptionIndex()
        {
            if (subscriptionIndex >= 0) return subscriptionIndex;

            var i = Array.IndexOf(subscribers, null);
            if (i >= 0) subscribers[i] = this;
            subscriptionIndex = i;

            return i;
        }

        public void _SubscribeBoolValue(FlightDataBoolValueId id)
        {
            boolSubscriptionMaskList[GetSubscriptionIndex()] |= GetMask((int)id);
        }
        public void _SubscribeFloatValue(FlightDataFloatValueId id)
        {
            var i = GetSubscriptionIndex();
            floatSubscriptionMaskList[i] |= GetMask((int)id);
        }
        public void _SubscribeVector3Value(FlightDataVector3ValueId id)
        {
            var i = GetSubscriptionIndex();
            vector3SubscriptionMaskList[i] |= GetMask((int)id);
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

        public bool _ReadBoolValue(FlightDataBoolValueId id)
        {
            return bools[(int)id];
        }
        public void _WriteBoolValue(FlightDataBoolValueId id, bool value)
        {
            bools[(int)id] = value;
        }
        public void _WriteBoolValueAndNotify(FlightDataBoolValueId id, bool value)
        {
            _WriteBoolValue(id, value);
            SendNotify((int)id, floatSubscriptionMaskList, nameof(_OnBoolValueChanged));
        }
        public virtual void _OnBoolValueChanged() { }

        public float _ReadFloatValue(FlightDataFloatValueId id)
        {
            return floats[(int)id];
        }
        public void _WriteFloatValue(FlightDataFloatValueId id, float value)
        {
            floats[(int)id] = value;
        }
        public void _WriteFloatValueAndNotify(FlightDataFloatValueId id, float value)
        {
            _WriteFloatValue(id, value);
            SendNotify((int)id, floatSubscriptionMaskList, nameof(_OnFloatValueChanged));
        }
        public virtual void _OnFloatValueChanged() { }

        public Vector3 _ReadVector3Value(FlightDataVector3ValueId id)
        {
            return vector3s[(int)id];
        }

        public void _WriteVector3Value(FlightDataVector3ValueId id, Vector3 value)
        {
            vector3s[(int)id] = value;
        }

        public void _WriteVector3ValueAndNotify(FlightDataVector3ValueId id, Vector3 value)
        {
            _WriteVector3Value(id, value);
            SendNotify((int)id, floatSubscriptionMaskList, nameof(_OnVector3ValueChanged));
        }

        public virtual void _OnVector3ValueChanged() { }
    }
}
