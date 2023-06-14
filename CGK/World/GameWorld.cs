using System;
using System.Collections.Generic;
using System.Linq;
using CGK.Event.Service;
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
            _eventDispatcher.AddListener<GameControllerDestroyedEvent>(
                GameControllerDestroyedEvent.GAME_CONTROLLER_DESTROYED, OnGameControllerDestroyed);
        }

        private void OnGameControllerDestroyed(GameControllerDestroyedEvent destroyedEvent)
        {
            _gameControllers.RemoveAll(r => r == destroyedEvent.ObjectToDestroy);
            GameObject.Destroy(destroyedEvent.ObjectToDestroy.gameObject);
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
            _eventDispatcher.RemoveListener<GameControllerDestroyedEvent>(
                GameControllerDestroyedEvent.GAME_CONTROLLER_DESTROYED, OnGameControllerDestroyed);
        }
    }
}