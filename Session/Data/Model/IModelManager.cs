using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IModelManager<T> : IModelManager
{
    Dictionary<string, T> Models { get; }
}

public interface IModelManager
{
    
}