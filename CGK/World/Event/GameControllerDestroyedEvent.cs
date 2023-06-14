using CGK.Event.Model;

namespace CGK.World.Event
{
    public class GameControllerDestroyedEvent : IEventModel
    {
        public const string GAME_CONTROLLER_DESTROYED = "gameControllerDestroyed";
        public string EventName => GAME_CONTROLLER_DESTROYED;

        private readonly GameController _objectToDestroy;

        public GameControllerDestroyedEvent(GameController objectToDestroy)
        {
            _objectToDestroy = objectToDestroy;
        }

        public GameController ObjectToDestroy => _objectToDestroy;
    }
}