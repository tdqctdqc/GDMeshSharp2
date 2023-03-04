using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IModel
{
    string Name { get; }
}

public static class IModelExt
{
    public static ModelRef<T> GetRef<T>(this T model) where T : IModel
    {
        return new ModelRef<T>(model.Name);
    }
}