using System;
using System.Collections.Generic;
using CommonGameKit.CommonGameKit.Assets.Dispatcher.Error;
using CommonGameKit.CommonGameKit.Assets.Dispatcher.Event;
using UnityEngine;

namespace CommonGameKit.CommonGameKit.Assets.Dispatcher.Service
{
    public delegate void EventHandlerDelegate<in T>(T data);

    public class EventDispatcher : MonoBehaviour
    {
        private readonly Dictionary<string, List<MulticastDelegate>> _events = new Dictionary<string, List<MulticastDelegate>>();

        public void AddListener<T>(string eventName, EventHandlerDelegate<T> callBack)
                where T : GameEvent
        {
            if (!_events.ContainsKey(eventName)) {
                _events.Add(eventName, new List<MulticastDelegate>() {
                        callBack
                });
                return;
            }
            _events[eventName].Add(callBack);
        }

        public void Dispatch<T>(T gameEvent)
                where T : GameEvent
        {
            if (!_events.ContainsKey(gameEvent.EventName)) {
                return;
            }
            List<MulticastDelegate> multicastDelegates = _events[gameEvent.EventName];
            for (int i = 0; i < multicastDelegates.Count; i++) {
                multicastDelegates[i].DynamicInvoke(gameEvent);
            }
        }

        public void RemoveListener<T>(string eventName, EventHandlerDelegate<T> callBack)
        {
            if (!_events.ContainsKey(eventName)) {
                throw new DispatcherException("Listner are removed or not registrated");
            }
            List<MulticastDelegate> multicastDelegates = _events[eventName];
            if (!multicastDelegates.Remove(callBack)) {
                throw new DispatcherException("Listner are removed or not registrated");
            }
        }
    }
}