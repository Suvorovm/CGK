using Client.Core.Event.Model;

namespace Client.Core.Snapshot.Event
{
    public class CreateSnapshotEvent : IEventModel
    {
        public const string CREATE_SNAPSHOT = "createSnapshot";
        public string EventName => CREATE_SNAPSHOT;
    }
}