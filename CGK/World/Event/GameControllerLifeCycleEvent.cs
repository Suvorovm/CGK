using CGK.Dispatcher.Event;

namespace CGK.World.Event
{
    public class GameControllerLifeCycleEvent : GameEvent
    {
        public const string GAME_CONTROLLER_DESTROY = "gameControllerDestroy";
        public const string GAME_CONTROLLER_PRE_GAME_OBJECT_DESTROYED = "preGameObjectDestroyed";
        public const string GAME_CONTROLLER_CREATED = "gameControllerCreated";

        private readonly GameController _gameController;


        public GameControllerLifeCycleEvent(string eventName, GameController gameController) : base(eventName)
        {
            _gameController = gameController;
        }

        public GameController GameController => _gameController;
    }
}