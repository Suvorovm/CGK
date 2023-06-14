using CGK.Dispatcher.Event;

namespace CGK.World.Event
{
    public class GameControllerDestroyedEvent : GameEvent
    {
        public const string GAME_CONTROLLER_DESTROYED = "gameControllerDestroyed";

        private readonly GameController _objectToDestroy;


        public GameControllerDestroyedEvent(string eventName, GameController objectToDestroy) : base(eventName)
        {
            _objectToDestroy = objectToDestroy;
        }

        public GameController ObjectToDestroy => _objectToDestroy;
    }
}