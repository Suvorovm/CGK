using System;
using System.Collections.Generic;
using Client.Core.Settings;
using Client.Core.Trigger.Descriptor;
using Client.Core.Trigger.Factory;
using UniRx;

namespace Client.Core.Trigger.Service
{
    public class TriggerService
    {
        private readonly TriggerFactoryAbstract _triggerFactory;
        private readonly List<ITrigger> _activatedTriggers;
        private readonly GameContext _gameContext;

        public TriggerService(TriggerFactoryAbstract triggerFactory, GameContext gameContext)
        {
            _triggerFactory = triggerFactory;
            _gameContext = gameContext;
            _activatedTriggers = new List<ITrigger>();
        }


        private ITrigger ActivateTrigger(ITrigger trigger, TriggerDescriptor triggerDescriptor)
        {
            trigger.Init(_gameContext, triggerDescriptor);
            _activatedTriggers.Add(trigger);

            CompositeDisposable compositeDisposable = new CompositeDisposable();
            IObservable<Unit> observable = trigger.OnResult.AsObservable();

            observable
                .Subscribe(_ =>
                {
                    compositeDisposable.Dispose();
                    _activatedTriggers.Remove(trigger);
                })
                .AddTo(compositeDisposable);

            return trigger;
        }

        public ITrigger CreateTrigger(TriggerDescriptor triggerDescriptor)
        {
            ITrigger trigger = _triggerFactory.CreatTrigger(triggerDescriptor.TriggerType);
            return ActivateTrigger(trigger, triggerDescriptor);
        }
    }
}