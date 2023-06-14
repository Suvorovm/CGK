using System;
using System.Collections.Generic;
using CGK.Snapshot.Model;
using CGK.Snapshot.Service;

namespace CGK.Snapshot.Factory
{
    public abstract class SnapshotFactoryAbstraction
    {
        protected readonly Dictionary<Type, ISnapshotService> _dictionarySnapshot = new Dictionary<Type, ISnapshotService>();

        public ISnapshotModel CreateSnapshotModelByType(Type type)
        {
            return _dictionarySnapshot[type].CreateSnapshotModel();
        }

        public void SetModel(Type type, ISnapshotModel value)
        {
            _dictionarySnapshot[type].LoadSnapshotModel(value);
        }
    }
}