using CGK.Event.Model;

namespace CGK.Snapshot.Event
{
    public class CreateSnapshotEvent : IEventModel
    {
        public const string CREATE_SNAPSHOT = "createSnapshot";
        public string EventName => CREATE_SNAPSHOT;
    }
}