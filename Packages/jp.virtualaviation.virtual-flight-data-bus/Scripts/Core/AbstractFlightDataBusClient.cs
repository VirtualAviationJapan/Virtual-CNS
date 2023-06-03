using System;
using UnityEngine;

namespace VirtualFlightDataBus
{
    /// <summary>
    /// Abstract base class for FlightDataBus clients.
    /// </summary>
    /// <remarks>
    /// A FlightDataBus client is a component that subscribes to and/or publishes values on a FlightDataBus.
    /// This class provides a set of methods to read, write, and subscribe to values on the bus.
    /// </remarks>
    abstract public class AbstractFlightDataBusClient : AbstractFlightDataBus
    {
        /// <summary>
        /// Gets the FlightDataBus instance that this client is subscribed to.
        /// </summary>
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

        /// <summary>
        /// This method is called after the flight data bus component has been initialized.
        /// </summary>
        protected virtual void OnStart() { }

        private void Subscribe(int id, ulong[] maskList, string eventName)
        {
            maskList[SubscriptionIndex] |= FlightDataUtilities.GetMask(id);
            SendCustomEvent(eventName);
        }

        /// <summary>
        /// Subscribes to a boolean value on the bus.
        /// </summary>
        /// <param name="id">The ID of the boolean value.</param>
        public void _Subscribe(FlightDataBoolValueId id) => Subscribe((int)id, boolSubscriptionMaskList, nameof(_OnBoolValueChanged));

        /// <summary>
        /// Subscribes to a float value on the bus.
        /// </summary>
        /// <param name="id">The ID of the float value.</param>
        public void _Subscribe(FlightDataFloatValueId id) => Subscribe((int)id, floatSubscriptionMaskList, nameof(_OnFloatValueChanged));

        /// <summary>
        /// Subscribes to a Vector3 value on the bus.
        /// </summary>
        /// <param name="id">The ID of the Vector3 value.</param>
        public void _Subscribe(FlightDataVector3ValueId id) => Subscribe((int)id, vector3SubscriptionMaskList, nameof(_OnVector3ValueChanged));

        /// <summary>
        /// Subscribes to a string value on the bus.
        /// </summary>
        /// <param name="id">The ID of the string value.</param>
        public void _Subscribe(FlightDataStringValueId id) => Subscribe((int)id, stringSubscriptionMaskList, nameof(_OnStringValueChanged));

        private void SendNotify(int id, ulong[] maskList, string eventName)
        {
            var mask = FlightDataUtilities.GetMask(id);
            for (var i = 0; i < maxSubscriberCount; i++)
            {
                var subscriber = subscribers[i];
                if (!subscriber) return;
                if ((maskList[i] & mask) == 0) continue;
                subscriber.SendCustomEvent(eventName);
            }
        }
        /// <summary>
        /// Reads a boolean flight data value.
        /// </summary>
        /// <param name="id">THe ID of the bool value.</param>
        /// <returns>The value of the specified bool.</returns>
        public bool _Read(FlightDataBoolValueId id) => bools[(int)id];
        /// <summary>
        /// Writes a bool value to the FlightDataBus.
        /// </summary>
        /// <param name="id">The ID of the bool value to write.</param>
        /// <param name="value">The value to write to the specified bool.</param>
        public void _Write(FlightDataBoolValueId id, bool value) => bools[(int)id] = value;

        /// <summary>
        /// Writes a bool value to the FlightDataBus and notifies subscribers of the update.
        /// </summary>
        /// <param name="id">The ID of the bool value to write.</param>
        /// <param name="value">The value to write to the specified bool.</param>
        public void _WriteAndNotify(FlightDataBoolValueId id, bool value)
        {
            _Write(id, value);
            SendNotify((int)id, boolSubscriptionMaskList, nameof(_OnBoolValueChanged));
        }
        /// <summary>
        /// Called by notification to value changed.
        /// </summary>
        public virtual void _OnBoolValueChanged() { }

        /// <summary>
        /// Reads the value of a float from the FlightDataBus.
        /// </summary>
        /// <param name="id">The ID of the float value to read.</param>
        /// <returns>The value of the specified float.</returns>
        public float _Read(FlightDataFloatValueId id) => floats[(int)id];
        /// <summary>
        /// Writes a float value to the FlightDataBus.
        /// </summary>
        /// <param name="id">The ID of the float value to write.</param>
        /// <param name="value">The value to write to the specified float.</param>
        public void _Write(FlightDataFloatValueId id, float value) => floats[(int)id] = value;

        /// <summary>
        /// Writes a float value to the FlightDataBus and notifies subscribers of the update.
        /// </summary>
        /// <param name="id">The ID of the float value to write.</param>
        /// <param name="value">The value to write to the specified float.</param>
        public void _WriteAndNotify(FlightDataFloatValueId id, float value)
        {
            _Write(id, value);
            SendNotify((int)id, floatSubscriptionMaskList, nameof(_OnFloatValueChanged));
            SendNotify((int)id / 3, vector3SubscriptionMaskList, nameof(_OnVector3ValueChanged));
        }
        /// <summary>
        /// Called by notification to value changed.
        /// </summary>
        public virtual void _OnFloatValueChanged() { }

        /// <summary>
        /// Reads the value of a Vector3 from the FlightDataBus.
        /// </summary>
        /// <param name="id">The ID of the Vector3 value to read.</param>
        /// <returns>The value of the specified Vector3.</returns>
        public void _Read(FlightDataVector3ValueId id, ref Vector3 value)
        {
            var i = (int)id;
            value.Set(floats[i], floats[i + 1], floats[i + 2]);
        }
        /// <summary>
        /// Writes a Vector3 value to the FlightDataBus.
        /// </summary>
        /// <param name="id">The ID of the Vector3 value to write.</param>
        /// <param name="value">The value to write to the specified Vector3.</param>
        public void _Write(FlightDataVector3ValueId id, in Vector3 value)
        {
            var i = (int)id;
            floats[i] = value.x;
            floats[i + 1] = value.y;
            floats[i + 2] = value.z;
        }
        /// <summary>
        /// Writes a Vector3 value to the FlightDataBus and notifies subscribers of the update.
        /// </summary>
        /// <param name="id">The ID of the Vector3 value to write.</param>
        /// <param name="value">The value to write to the specified Vector3.</param>
        public void _WriteAndNotify(FlightDataVector3ValueId id, in Vector3 value)
        {
            _Write(id, value);
            SendNotify((int)id, floatSubscriptionMaskList, nameof(_OnVector3ValueChanged));
        }
        /// <summary>
        /// Called by notification to value changed.
        /// </summary>
        public virtual void _OnVector3ValueChanged() { }

        /// <summary>
        /// Reads the value of a string from the FlightDataBus.
        /// </summary>
        /// <param name="id">The ID of the string value to read.</param>
        /// <returns>The value of the specified string.</returns>
        public string _Read(FlightDataStringValueId id) => strings[(int)id];
        /// <summary>
        /// Writes a string value to the FlightDataBus and notifies subscribers of the update.
        /// </summary>
        /// <param name="id">The ID of the string value to write.</param>
        /// <param name="value">The value to write to the specified string.</param>
        public void _WriteAndNotify(FlightDataStringValueId id, string value)
        {
            strings[(int)id] = value;
            SendNotify((int)id, stringSubscriptionMaskList, nameof(_OnStringValueChanged));
        }
        /// <summary>
        /// Called by notification to value changed.
        /// </summary>
        public virtual void _OnStringValueChanged() { }
    }
}
