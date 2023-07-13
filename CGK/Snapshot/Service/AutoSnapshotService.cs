using System;
using CGK.Descriptor.Service;
using CGK.Dispatcher.Service;
using CGK.Settings;
using CGK.Snapshot.Event;
using UniRx;
using UnityEngine;

namespace CGK.Snapshot.Service
{
    public class AutoSnapshotService : IDisposable
    {
        private readonly EventDispatcher _eventDispatcher;
        private readonly CompositeDisposable _compositeDisposable;

        public AutoSnapshotService(EventDispatcher eventDispatcher, DescriptorHolder holder)
        {
            _eventDispatcher = eventDispatcher;
            _compositeDisposable = new CompositeDisposable();
            GameConfig gameConfig = holder.GetDescriptor<GameConfig>();
            Observable.Interval(TimeSpan.FromSeconds(gameConfig.BuildSettings.AutoSaveTimeOutInSeconds))
                .Subscribe(_ => OnSaveTimeReached())
                .AddTo(_compositeDisposable);

        }

        private void OnSaveTimeReached()
        {
            Debug.Log("Auto Snapshot Created");
            _eventDispatcher.Dispatch<CreateSnapshotEvent>( new CreateSnapshotEvent(CreateSnapshotEvent.CREATE_SNAPSHOT));
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}