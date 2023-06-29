using CGK.Dispatcher.Event;

namespace CGK.World.Event
{
    public class GameControllerLifeCycleEvent : GameEvent
    {
        public const string GAME_CONTROLLER_DESTROYED = "gameControllerDestroyed";
        public const string GAME_CONTROLLER_CREATED = "gameControllerCreated";

        private readonly GameController _gameController;


        public GameControllerLifeCycleEvent(string eventName, GameController gameController) : base(eventName)
        {
            _gameController = gameController;
        }

        public GameController GameController => _gameController;
    }
}