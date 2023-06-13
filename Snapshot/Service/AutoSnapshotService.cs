using System;
using Client.Core.Descriptor.Service;
using Client.Core.Event.Service;
using Client.Core.Settings;
using Client.Core.Snapshot.Event;
using UniRx;
using UnityEngine;

namespace Client.Core.Snapshot.Service
{
    public class AutoSnapshotService : IDisposable
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly CompositeDisposable _compositeDisposable;

        public AutoSnapshotService(EventDispatcher eventDispatcher, DescriptorService descriptorService)
        {
            _eventDispatcher = eventDispatcher;
            _compositeDisposable = new CompositeDisposable();
            GameConfig gameConfig = descriptorService.GetDescriptor<GameConfig>();
            Observable.Interval(TimeSpan.FromSeconds(gameConfig.BuildSettings.GameSettings.AutoSaveTimeOutInSeconds))
                .Subscribe(_ => OnSaveTimeReached())
                .AddTo(_compositeDisposable);

        }

        private void OnSaveTimeReached()
        {
            Debug.Log("Auto Snapshot Created");
            _eventDispatcher.Dispatch<CreateSnapshotEvent>( new CreateSnapshotEvent());
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}