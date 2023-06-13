using System;
using System.Collections.Generic;
using System.Linq;
using Client.Core.Event.Exception;
using Client.Core.Event.Model;

namespace Client.Core.Event.Service
{
    public class EventDispatcher 
    {
        private readonly Dictionary<string, List<MulticastDelegate>> _events =
            new Dictionary<string, List<MulticastDelegate>>();

        public void AddListener<T>(string eventName, Action<T> callBack)
            where T : IEventModel
        {
            if (!_events.ContainsKey(eventName))
            {
                _events.Add(eventName, new List<MulticastDelegate>()
                {
                    callBack
                });
                return;
            }

            _events[eventName].Add(callBack);
        }

        public void Dispatch<T>(T gameEvent)
            where T : IEventModel
        {
            if (!_events.ContainsKey(gameEvent.EventName))
            {
                return;
            }

            List<MulticastDelegate> multicastDelegates = _events[gameEvent.EventName];
            for (int i = 0; i < multicastDelegates.Count; i++)
            {
                multicastDelegates[i].DynamicInvoke(gameEvent);
            }
        }

        public void RemoveListener<T>(string eventName, Action<T> callBack)
        {
            if (!_events.ContainsKey(eventName))
            {
                throw new DispatcherException("Listener are removed or not registrated");
            }

            List<MulticastDelegate> multicastDelegates = _events[eventName];
            if (!multicastDelegates.Remove(callBack))
            {
                throw new DispatcherException("Listener are removed or not registrated");
            }
        }
        public bool HasListener<T>(string eventName, Action<T> callBack)
        {
            if (!_events.ContainsKey(eventName))
            {
                return false;
            }

            List<MulticastDelegate> multicastDelegates = _events[eventName];
            return multicastDelegates.Contains(callBack);
        }
        
    }
}