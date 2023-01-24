using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Models
{
    private Dictionary<Type, IModelRepo> _repos;
    public LandformManager Landforms { get; private set; }
    public VegetationManager Vegetation { get; private set; }
    public Models()
    {
        _repos = new Dictionary<Type, IModelRepo>();
        Landforms = new LandformManager();
        AddRepo(Landforms);
        Vegetation = new VegetationManager();
        AddRepo(Vegetation);
    }

    public T GetModel<T>(string name)
    {
        var repo = (IModelRepo<T>) _repos[typeof(T)];
        return repo.Models[name];
    }

    private void AddRepo<T>(IModelRepo<T> repo)
    {
        _repos.Add(typeof(T), repo);
    }
}