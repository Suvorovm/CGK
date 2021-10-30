namespace CommonGameKit.CommonGameKit.Assets.Dispatcher.Event
{
    public class GameEvent
    {
        private readonly string _eventName;

        protected GameEvent(string eventName)
        {
            _eventName = eventName;
        }

        public string EventName
        {
            get { return _eventName; }
        }
    }
}