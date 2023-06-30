using System;
using System.Collections.Generic;
using System.Linq;
using CGK.Dispatcher.Service;
using CGK.World.Event;
using UnityEngine;

namespace CGK.World
{
    public class GameWorld : IDisposable
    {
        private readonly EventDispatcher _eventDispatcher;
        private GameObject _rootObject;
        private List<GameController> _gameControllers;

        public GameWorld(EventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            _eventDispatcher.AddListener<GameControllerLifeCycleEvent>(
                GameControllerLifeCycleEvent.GAME_CONTROLLER_DESTROY, OnGameControllerDestroyed);
            _eventDispatcher.AddListener<GameControllerLifeCycleEvent>(
                GameControllerLifeCycleEvent.GAME_CONTROLLER_CREATED, OnGameControllerCreated);
        }

        private void OnGameControllerCreated(GameControllerLifeCycleEvent gameEvent)
        {
            _gameControllers.Add(gameEvent.GameController);
        }

        private void OnGameControllerDestroyed(GameControllerLifeCycleEvent lifeCycleEvent)
        {
            _gameControllers.RemoveAll(r => r == lifeCycleEvent.GameController);
            _eventDispatcher.Dispatch<GameControllerLifeCycleEvent>(new GameControllerLifeCycleEvent(
                GameControllerLifeCycleEvent.GAME_CONTROLLER_PRE_GAME_OBJECT_DESTROYED, lifeCycleEvent.GameController));
            GameObject.Destroy(lifeCycleEvent.GameController.gameObject);
        }

        public void Init(GameObject root)
        {
            _rootObject = root;
            _gameControllers = _rootObject.GetComponentsInChildren<GameController>().ToList();
        }

        public T[] GetComponents<T>()
        {
            return _gameControllers
                .Select(x => x.GetComponent<T>())
                .Where(x => x != null)
                .ToArray();
        }

        public T GetComponent<T>()
        {
            return _gameControllers
                .Select(c => c.GetComponent<T>())
                .First(c => c != null);
        }

        public void AddObject<T>(T gameController)
            where T : GameController
        {
            _gameControllers.Add(gameController);
        }

        public void Dispose()
        {
            _gameControllers?.Clear();
            _eventDispatcher.RemoveListener<GameControllerLifeCycleEvent>(
                GameControllerLifeCycleEvent.GAME_CONTROLLER_DESTROY, OnGameControllerDestroyed);
            _eventDispatcher.RemoveListener<GameControllerLifeCycleEvent>(
                GameControllerLifeCycleEvent.GAME_CONTROLLER_CREATED, OnGameControllerCreated);
        }
    }
}