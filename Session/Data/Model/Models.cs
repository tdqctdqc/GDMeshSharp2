using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Models
{
    private Dictionary<Type, IModelRepo> _repos;
    private Dictionary<string, object> _models;
    public LandformManager Landforms { get; private set; }
    public VegetationManager Vegetation { get; private set; }
    public Models()
    {
        _repos = new Dictionary<Type, IModelRepo>();
        _models = new Dictionary<string, object>();
        Landforms = new LandformManager();
        AddRepo(Landforms);
        Vegetation = new VegetationManager();
        AddRepo(Vegetation);
    }

    public T GetModel<T>(string name)
    {
        return (T)_models[name];
    }

    private void AddRepo<T>(IModelRepo<T> repo)
    {
        _repos.Add(typeof(T), repo);
        foreach (var keyValuePair in repo.Models)
        {
            _models.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }
}