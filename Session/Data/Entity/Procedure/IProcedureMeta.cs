using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IProcedureMeta
{
    Procedure Deserialize(object[] args);
    void Initialize(Procedure u, object[] args);
    object[] GetArgs(Procedure update);
}