using CGK.Snapshot.Model;

namespace CGK.Snapshot.Service
{
    public interface ISnapshotService
    {
        ISnapshotModel CreateSnapshotModel();

        void LoadSnapshotModel(ISnapshotModel snapshotModel);
    }
}