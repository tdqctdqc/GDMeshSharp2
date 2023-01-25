using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ModelRef<T> : IRef<string> where T : IModel
{
    public string ModelName { get; private set; }
    private T _ref;

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
    public static ModelRef<T> DeserializeConstructor(string t, ServerWriteKey key)
    {
        return new ModelRef<T>(t);
    }
    string IRef<string>.GetUnderlying() => ModelName;
}