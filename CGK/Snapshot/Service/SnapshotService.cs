using System;
using System.IO;
using System.Reflection;
using CGK.Dispatcher.Service;
using CGK.Snapshot.Event;
using CGK.Snapshot.Factory;
using CGK.Snapshot.Model;
using CGK.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace CGK.Snapshot.Service
{
    public class SnapshotService<T> : IDisposable
        where T : new()

    {
    private readonly string _fileSavedDir;

    private readonly SnapshotFactoryAbstraction _factory;
    private readonly EventDispatcher _eventDispatcher;


    public SnapshotService(SnapshotFactoryAbstraction factory, EventDispatcher eventDispatcher, string fileSavedDir)
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
        FieldInfo[] fieldInfos = typeof(T).GetFields();
        T gameSnapshot = new T();
        foreach (FieldInfo property in fieldInfos)
        {
            ISnapshotModel snapshotModel = _factory.CreateSnapshotModelByType(property.FieldType);
            property.SetValue(gameSnapshot, snapshotModel);
        }

        WriteToFile(gameSnapshot);
    }

    private void WriteToFile(T gameSnapshot)
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
        T gameSnapshot = ReadSaveFile();

        LoadSnapshot(gameSnapshot);
    }

    private void LoadSnapshot(T gameSnapshot)
    {
        FieldInfo[] fieldInfos = typeof(T).GetFields();

        foreach (FieldInfo property in fieldInfos)
        {
            object propertyValue = property.GetValue(gameSnapshot);
            Type typeProperty = propertyValue.GetType();
            _factory.SetModel(typeProperty, (ISnapshotModel)propertyValue);
        }
    }

    private T ReadSaveFile()
    {
        string jsonText = File.ReadAllText(GetPath());

        return JsonConvert.DeserializeObject<T>(jsonText);
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
        LoadSnapshot(new T());
    }

    public void Dispose()
    {
        _eventDispatcher.RemoveListener<CreateSnapshotEvent>(CreateSnapshotEvent.CREATE_SNAPSHOT,
            OnCreateSnapshotEvent);
    }
    }
}