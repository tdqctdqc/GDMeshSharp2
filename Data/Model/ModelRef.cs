using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

[RefAttribute] 
public class ModelRef<T> : IRef<string> where T : IModel
{
    public string ModelName { get; private set; }
    private T _ref;

    public ModelRef(T model, CreateWriteKey key)
    {
        ModelName = model.Name;
    }

    public ModelRef(string modelName)
    {
        ModelName = modelName;
    }

    public T Ref()
    {
        if (_ref == null)
        {
            Game.I.RefFulfiller.Fulfill(this);
        }

        return _ref;
    }

    public void SyncRef(Data data)
    {
        _ref = data.Models.GetModel<T>(ModelName);
    }
    public static ModelRef<T> DeserializeConstructor(string t)
    {
        return new ModelRef<T>(t);
    }
    public string GetUnderlying() => ModelName;
    public void Set(string underlying, StrongWriteKey key)
    {
        ModelName = underlying;
    }
}