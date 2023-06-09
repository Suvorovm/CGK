using System;
using System.IO;
using System.Reflection;
using Client.Core.Event.Service;
using Client.Core.Snapshot.Event;
using Client.Core.Snapshot.Factory;
using Client.Core.Snapshot.Model;
using Client.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Client.Core.Snapshot.Service
{
    public class SnapshotService : IDisposable
    {
        private readonly string _fileSavedDir;

        private readonly SnapshotFactory _factory;
        private readonly EventDispatcher _eventDispatcher;


        public SnapshotService(SnapshotFactory factory, EventDispatcher eventDispatcher, string fileSavedDir)
        {
            _eventDispatcher = eventDispatcher;
            _fileSavedDir = fileSavedDir;
            _factory = factory;
            _eventDispatcher.AddListener<CreateSnapshotEvent>(CreateSnapshotEvent.CREATE_SNAPSHOT, OnCreateSnapshotEvent);
        }

        private void OnCreateSnapshotEvent(CreateSnapshotEvent eventModel)
        {
            CreateSnapshot();
        }

        public bool HasValidSnapshot()
        {
            return HasFile(GetPath());
        }

        public void CreateSnapshot()
        {
            FieldInfo[] fieldInfos = typeof(GameSnapshot).GetFields();
            GameSnapshot gameSnapshot = new GameSnapshot();
            foreach (FieldInfo property in fieldInfos)
            {
                ISnapshotModel snapshotModel = _factory.CreateSnapshotModelByType(property.FieldType);
                property.SetValue(gameSnapshot, snapshotModel);
            }

            WriteToFile(gameSnapshot);
        }

        private void WriteToFile(GameSnapshot gameSnapshot)
        {
            string json = JsonConvert.SerializeObject(gameSnapshot);
            Exception exception;
            try
            {
                var path = GetPath();
                FileHandling.CreateDirectoryIfDoesntExistAndWriteAllText(path, json,
                    out exception);
            }
            catch (Exception ex)
            {
                exception = ex;
                Debug.LogError("Exception " + ex.Message);
            }
        }

        public void LoadSnapshot()
        {
            GameSnapshot gameSnapshot = ReadSaveFile();

            LoadSnapshot(gameSnapshot);
        }

        private void LoadSnapshot(GameSnapshot gameSnapshot)
        {
            FieldInfo[] fieldInfos = typeof(GameSnapshot).GetFields();

            foreach (FieldInfo property in fieldInfos)
            {
                object propertyValue = property.GetValue(gameSnapshot);
                Type typeProperty = propertyValue.GetType();
                _factory.SetModel(typeProperty, (ISnapshotModel) propertyValue);
            }
        }

        private GameSnapshot ReadSaveFile()
        {
            string jsonText = File.ReadAllText(GetPath());

            return JsonConvert.DeserializeObject<GameSnapshot>(jsonText);
        }

        private string GetPath()
        {
            return Application.persistentDataPath + _fileSavedDir;
        }

        private bool HasFile(string path)
        {
            return File.Exists(path);
        }

        public void CreateDefaultSnapshot()
        {
            LoadSnapshot(new GameSnapshot());
        }

        public void Dispose()
        {
            _eventDispatcher.RemoveListener<CreateSnapshotEvent>(CreateSnapshotEvent.CREATE_SNAPSHOT, OnCreateSnapshotEvent);
        }
    }
}