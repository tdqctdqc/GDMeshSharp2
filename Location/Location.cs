using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;

public abstract class Location : Entity
{
    [JsonConstructor] public Location(int id) : base(id)
    {
    }

    protected Location(int id, CreateWriteKey key) : base(id, key)
    {
    }
    
}