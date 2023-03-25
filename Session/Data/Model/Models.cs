using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Models
{
    private Dictionary<Type, IModelManager> _managers;
    public IModel this[string name] => _models.TryGetValue(name, out var val) 
        ? (IModel) val
        : null;
    public Dictionary<string, object> _models;
    public LandformManager Landforms { get; private set; }
    public VegetationManager Vegetation { get; private set; }
    public PeepJobManager PeepJobs { get; private set; }
    public ItemManager Items { get; private set; }
    public BuildingModelManager Buildings { get; private set; }
    public RoadModelManager Roads { get; private set; }
    public Models()
    {
        _managers = new Dictionary<Type, IModelManager>();
        _models = new Dictionary<string, object>();
        Landforms = new LandformManager();
        AddManager(Landforms);
        Vegetation = new VegetationManager();
        AddManager(Vegetation);
        PeepJobs = new PeepJobManager();
        AddManager(PeepJobs);
        Items = new ItemManager();
        AddManager(Items);
        Buildings = new BuildingModelManager();
        AddManager(Buildings);
        Roads = new RoadModelManager();
        AddManager(Roads);
    }

    public T GetModel<T>(string name)
    {
        return (T)_models[name];
    }

    public IModelManager<TModel> GetManager<TModel>() where TModel : IModel
    {
        return (IModelManager<TModel>)_managers[typeof(TModel)];
    }
    private void AddManager<T>(IModelManager<T> manager)
    {
        _managers.Add(typeof(T), manager);
        foreach (var keyValuePair in manager.Models)
        {
            _models.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }
}