using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ModelRefCollection<TModel> : IRefCollection<string>
    where TModel : IModel
{
    public List<string> ModelNames { get; private set; }
    private List<TModel> _refs;
    public static ModelRefCollection<TModel> Construct()
    {
        return new ModelRefCollection<TModel>(new List<string>());
    }
    [SerializationConstructor] private ModelRefCollection(List<string> modelNames)
    {
        ModelNames = modelNames;
        _refs = null;
        Game.I.RefFulfiller.Fulfill(this);
    }
    public IReadOnlyCollection<TModel> Models()
    {
        if (_refs == null)
        {
            Game.I.RefFulfiller.Fulfill(this);
        }
        return _refs;
    }

    public void SyncRef(Data data)
    {
        _refs = new List<TModel>();
        foreach (var name in ModelNames)
        {
            TModel refer = data.Models.GetManager<TModel>().Models[name];
            _refs.Add(refer);
        }
    }

    public void ClearRef()
    {
        ModelNames = new List<string>();
        _refs = null;
    }

    public void AddGen(string name, GenWriteKey key)
    {
        ModelNames.Add(name);
        _refs.Add(key.Data.Models.GetManager<TModel>().Models[name]);
    }
    public void AddByProcedure(List<string> names, ProcedureWriteKey key)
    {
        for (var i = 0; i < names.Count; i++)
        {
            ModelNames.Add(names[i]);
            _refs.Add(key.Data.Models.GetManager<TModel>().Models[names[i]]);
        }
    }

    public void AddByProcedure(string name, ProcedureWriteKey key)
    {
        ModelNames.Add(name);
        _refs.Add(key.Data.Models.GetManager<TModel>().Models[name]);
    }

    public void RemoveByProcedure(List<string> names, ProcedureWriteKey key)
    {
        for (var i = 0; i < names.Count; i++)
        {
            ModelNames.Remove(names[i]);
            _refs.Remove(key.Data.Models.GetManager<TModel>().Models[names[i]]);
        }
    }

    public void RemoveByProcedure(string name, ProcedureWriteKey key)
    {
        ModelNames.Remove(name);
        _refs.Remove(key.Data.Models.GetManager<TModel>().Models[name]);
    }

    public bool Contains(TModel t)
    {
        return ModelNames.Contains(t.Name);
    }


}
