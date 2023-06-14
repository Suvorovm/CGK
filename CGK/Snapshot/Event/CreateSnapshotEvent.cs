
using CGK.Dispatcher.Event;

namespace CGK.Snapshot.Event
{
    public class CreateSnapshotEvent : GameEvent
    {
        public const string CREATE_SNAPSHOT = "createSnapshot";

        public CreateSnapshotEvent(string eventName) : base(eventName)
        {
        }
    }
}