using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IModelRepo<T> : IModelRepo
{
    Dictionary<string, T> Models { get; }
}

public interface IModelRepo
{
    
}