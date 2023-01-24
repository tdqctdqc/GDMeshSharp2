using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ModelRef<T> : IRef
{
    public string ModelName { get; private set; }
    private T _ref;

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
}