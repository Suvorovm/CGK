using System;
using System.Collections.Generic;

namespace Client.Core.Trigger.Factory
{
    public abstract class TriggerFactoryAbstract
    {
        protected Dictionary<string, Type> _triggerPool;

        public ITrigger CreatTrigger(string triggerType)
        {
            Type type = _triggerPool[triggerType];
            ITrigger trigger = (ITrigger) Activator.CreateInstance(type);
            return trigger;
        }
    }
}