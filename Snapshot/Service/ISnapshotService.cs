using Client.Core.Snapshot.Model;

namespace Client.Core.Snapshot.Service
{
    public interface ISnapshotService
    {
        ISnapshotModel CreateSnapshotModel();

        void LoadSnapshotModel(ISnapshotModel snapshotModel);
    }
}